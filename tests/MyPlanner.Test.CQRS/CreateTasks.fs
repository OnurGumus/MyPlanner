module MyPlanner.Test.CQRS.CreateTasks

open TickSpec
open MyPlanner.Command.API
open MyPlanner.Shared.Domain
open System.Threading
open MyPlanner.Query
open System.IO
open Akkling
open Expecto

let config =
    Configuration.parse (
        """
    akka {
        persistence{
            query.journal.sql {
            # Implementation class of the SQL ReadJournalProvider
            class = "Akka.Persistence.Query.Sql.SqlReadJournalProvider, Akka.Persistence.Query.Sql"
            # Absolute path to the write journal plugin configuration entry that this
            # query journal will connect to.
            # If undefined (or "") it will connect to the default journal as specified by the
            # akka.persistence.journal.plugin property.
            write-plugin = ""
            # The SQL write journal is notifying the query side as soon as things
            # are persisted, but for efficiency reasons the query side retrieves the events
            # in batches that sometimes can be delayed up to the configured `refresh-interval`.
            refresh-interval = 1s
            # How many events to fetch in one query (replay) and keep buffered until they
            # are delivered downstreams.
            max-buffer-size = 20
            }
            journal {
              plugin = "akka.persistence.journal.sqlite"
              sqlite
              {
                  connection-string = "Data Source=InMemorySample;Mode=Memory;Cache=Shared;"
                  auto-initialize = on
                  event-adapters.tagger = "MyPlanner.Command.Actor+Tagger, MyPlanner.Command"
                  event-adapter-bindings {
                    "MyPlanner.Command.Common+IDefaultTag, MyPlanner.Command" = tagger
                  }
              }
            }
          snapshot-store{
            plugin = "akka.persistence.snapshot-store.sqlite"
            sqlite {
                auto-initialize = on
                connection-string = "Data Source=InMemorySample;Mode=Memory;Cache=Shared"
            }
          }
        }
        extensions = ["Akka.Cluster.Tools.PublishSubscribe.DistributedPubSubExtensionProvider,Akka.Cluster.Tools"]
        stdout-loglevel = INFO
           loglevel = INFO
           log-config-on-start = on

        actor {
        debug {
                              receive = on
                              autoreceive = on
                              lifecycle = on
                              event-stream = on
                              unhandled = on
                        }
            provider = "Akka.Cluster.ClusterActorRefProvider, Akka.Cluster"
            serializers {
                json = "Akka.Serialization.NewtonSoftJsonSerializer"
                plainnewtonsoft = "MyPlanner.Command.Common+PlainNewtonsoftJsonSerializer, MyPlanner.Command"
            }
            serialization-bindings {
                "System.Object" = json
                "MyPlanner.Command.Common+IDefaultTag, MyPlanner.Command" = plainnewtonsoft
            }
        }
        remote {
            dot-netty.tcp {
                public-hostname = "localhost"
                hostname = "localhost"
                port = 0
            }
        }
        cluster {
            auto-down-unreachable-after = 5s
           # sharding.remember-entities = true
        }

        }
    }
    """
    )

[<Given>]
let ``there are no tasks in the system`` () =
    let connString =
        "Data Source=InMemorySample;Mode=Memory;Cache=Shared"

    let conn =
        MyPlanner.Query.Projection.createTables (connString)

    let commandApi =
        MyPlanner.Command.API.api config NodaTime.SystemClock.Instance

    let queryApi =
        MyPlanner.Query.API.api connString commandApi.ActorApi

    conn, commandApi, queryApi


[<When>]
let ``I create a task`` (api: IAPI) =
    api.CreateTask
        { Id = TaskId "test_task"
          Version = version0 }
    |> Async.RunSynchronously

[<Then>]
let ``the task should be created successfully`` (result: Result<Task, string>) =
    match result with
    | Ok _ -> ()
    | Error _ -> "task failed" |> Expect.isTrue false

[<When>]
let ``I visit url /tasks`` (queryApi: MyPlanner.Query.API.IAPI) =
    Thread.Sleep 3000
    queryApi.Query<Task>() |> Async.RunSynchronously

[<Then>]
let ``I should see 1 task\(s\) listed`` (tasks: Task list, commandApi: MyPlanner.Command.API.IAPI) =
    Expect.equal tasks.Length 1 "more than 1 tasks"
    commandApi.ActorApi.Stop().Wait()
