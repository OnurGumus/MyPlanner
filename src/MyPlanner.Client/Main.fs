module MyPlanner.Client.Main

open MyPlanner.Shared.Msg
open Elmish
open MyPlanner.Client.Pages

type Route =
    | Signin
    | Signup

type Page =
    | Signin of Signin.Model
    | Signup of Signup.Model

type ConnectionStatus =
    | Connected
    | Disconnected

type Model =
    {
        Page: Page option
        ConnectionStatus: ConnectionStatus
    }

type Msg =
    | ServerDisconnected
    | Remote of ServerToClient.Msg
    | SigninMsg of Signin.Msg
    | SignupMsg of Signup.Msg

let startPage = Route.Signin |> Some

let initSignin model : Model * Cmd<_> =
    let (signInModel: Signin.Model), cmd = Signin.init ()
    let page = Signin signInModel
    { model with Page = Some page }, (Cmd.map SigninMsg cmd)

let initSignup model : Model * Cmd<_> =
    let (signUpModel: Signup.Model), cmd = Signup.init ()
    let page = Signup signUpModel
    { model with Page = Some page }, (Cmd.map SignupMsg cmd)

//called externally when url changed from the code at runtime.
//not called when you land to a page if you type it to the address bar
let urlUpdate newUrl toPage bridgeSend (result: Route option) (model: Model) =
    match result, model.Page with
    //we are already at tasks page, so no action required.
    | Some (Route.Signin), Some (Page.Signin _) -> model, Cmd.none
    //we were at some other page but now redirecting to signup. redundant if we have only 1 page.
    | Some (Route.Signin), Some _ -> initSignin model
    | Some (Route.Signup), _ -> initSignup model

    //initial landing handled below
    | Some (Route.Signin), None ->
        let model, signinCmd = initSignin model

        model,
        Cmd.batch [
            signinCmd
            Cmd.ofSub (fun _ -> newUrl ((toPage Route.Signin), true))
        ]
    | other ->
        printf "%A" other
        failwith "invalid url"

let init newUrl toPage (bridgeSend: ClientToServer.Msg -> unit) (page: Route option) =
    urlUpdate
        newUrl
        toPage
        bridgeSend
        page
        {
            Page = None
            ConnectionStatus = Disconnected
        }

let update (bridgeSend: ClientToServer.Msg -> unit) newUrl toPage msg model =
    match msg, model with
    | Remote (ServerToClient.ServerConnected), _ ->
        let model =
            { model with
                ConnectionStatus = Connected
            }
        // //we postpone the actual init of page till connection established
        // match model.Page.Value with
        // | (Signin _) -> initSignin model
        // | _ -> model, Cmd.none //page is already init
        model, Cmd.none

    | ServerDisconnected, _ ->
        { model with
            ConnectionStatus = Disconnected
        },
        Cmd.none

    | SigninMsg msg, { Page = Some (Signin signinModel) } ->
        let newModel, cmd = Signin.update msg signinModel

        { model with
            Page = Some(Signin(newModel))
        },
        Cmd.map SigninMsg cmd

    | msg, model ->
        invalidOp
        <| sprintf "not supported case %A %A" msg model

let mapClientMsg msg =
    match msg with
    | _ -> Remote msg
