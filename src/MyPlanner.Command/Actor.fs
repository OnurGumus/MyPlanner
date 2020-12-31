module MyPlanner.Command.Actor

open Akkling
open Akka.Cluster.Tools.Singleton
open Akka.Streams
open Akka.Persistence.Journal
open System.Collections.Immutable
open System

let configWithPort port =
    let config =
        Configuration.parse
            ("""
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
                      connection-string = "Data Source=test.db;"
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
                    connection-string = "Data Source=test.db"
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
                    port = """
             + port.ToString()
             + """
                }
            }
            cluster {
                auto-down-unreachable-after = 5s
               # sharding.remember-entities = true
            }

            }
        }
        """)

    config.WithFallback(ClusterSingletonManager.DefaultConfig())


let defaultTag = ImmutableHashSet.Create("default")

type Tagger =
    interface IWriteEventAdapter with
        member _.Manifest _ = ""
        member _.ToJournal evt = 
            box <| Tagged(evt, defaultTag)

    new() = {  }

let system =
    System.create "cluster-system" (configWithPort 0)

open Akka.Cluster

Cluster.Get(system).SelfAddress
|> Cluster.Get(system).Join

open Akka.Cluster.Tools.PublishSubscribe
open Akkling.Persistence
open Akka.Persistence.Sqlite

let mediator = DistributedPubSub.Get(system).Mediator

let mat = ActorMaterializer.Create(system)


SqlitePersistence.Get(system) |> ignore

let subscribeForCommand command =
    Common.CommandHandler.subscribeForCommand system (typed mediator) command