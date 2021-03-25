module MyPlanner.Shared.Msg

module ServerToClient =
    type TasksMsg =
        | TasksFetched of Domain.Task list
        | TaskCreateCompleted of Result<Domain.Task,string>
        | TaskCreated of Domain.Task

    type Msg =
        | ServerConnected
        | TasksMsg of TasksMsg

module ClientToServer =
    type TasksMsg =
        | TasksRequested
        | TaskCreationRequested of Domain.Task

    type Msg = TasksMsg of TasksMsg
