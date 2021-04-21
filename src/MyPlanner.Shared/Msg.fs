module MyPlanner.Shared.Msg

module ServerToClient =

    type Msg = ServerConnected

module ClientToServer =
    type TasksMsg =
        | TasksRequested

    type Msg = TasksMsg of TasksMsg
