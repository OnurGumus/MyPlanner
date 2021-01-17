module MyPlanner.Shared.Msg

module ServerToClient =
    type TasksMsg = TasksFetched of Domain.Task list

    type Msg =
        | ServerConnected
        | TasksMsg of TasksMsg

module ClientToServer =
    type TasksMsg = TasksRequested
    type Msg = TasksMsg of TasksMsg
