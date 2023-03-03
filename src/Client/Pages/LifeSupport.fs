namespace Pages


module LifeSupport =

    open Feliz
    open Feliz.UseElmish
    open Elmish
    open MaterialUI5
    open Shared
    open Views
    open Utils


    type State = { Dialog: string list }


    type Msg =
        | RowClick of int * string list
        | CloseDialog


    let init () = { Dialog = [] }, Cmd.none


    let update (msg: Msg) state =
        match msg with
        | RowClick (i, xs) ->
            Logging.log "rowclick:" i
            { state with Dialog = xs }, Cmd.none
        | CloseDialog -> { state with Dialog = [] }, Cmd.none


    let useStyles =
        Styles.makeStyles (fun styles theme ->
            {|
                container =
                    styles.create [
                        style.boxSizing.borderBox
                    ]
                mainview =
                    styles.create [
                        style.display.flex
                        style.overflow.hidden
                        style.flexDirection.column
                        style.marginLeft 20
                        style.marginRight 20
                    ]
                patientpanel =
                    styles.create [
                        style.marginTop 70
                        style.top 0
                    ]
                patientdetails =
                    styles.create [
                        style.display.flex
                        style.flexDirection.row
                    ]
            |}
        )


    [<ReactComponent>]
    let View (input: {| interventions: Deferred<Intervention list> |}) =
        let lang =
            React.useContext (Global.languageContext)

        let state, dispatch =
            React.useElmish (init, update, [||])

        let classes_ = useStyles ()

        let emergencyList =
            Html.div [
                prop.id "emergency-list"
                prop.children [
                    Html.div [
                        // prop.className classes.patientdetails
                        prop.style [ style.flexGrow 1 ]
                        prop.children [
                            match input.interventions with
                            | Resolved (meds) ->
                                EmergencyList.render meds (RowClick >> dispatch)
                            | _ ->
                                Html.div [
                                    prop.style [ style.paddingTop 20 ]
                                    prop.children [
                                        Utils.Typography.h3 (
                                            Localization.Terms.``Emergency List``
                                            |> Localization.getTerm lang
                                        )
                                        Utils.Typography.h5 (
                                            Localization.Terms.``Emergency List show when patient data``
                                            |> Localization.getTerm lang
                                        )
                                    ]
                                ]
                        ]
                    ]
                ]
            ]

        let dialog =
            let content =
                state.Dialog
                |> function
                    | [ ind; int; calc; prep; adv ] ->
                        [ ind; int; calc; prep; adv ]
                        |> List.zip [
                            "indicatie"
                            "interventie"
                            "berekend"
                            "toediening"
                            "advies"
                           ]
                        |> List.collect (fun (s1, s2) ->
                            if s2 = "" then
                                []
                            else
                                [
                                    Mui.listItem [
                                        Mui.listItemText [
                                            listItemText.primary s1
                                            if s1 = "interventie"
                                               || s1 = "toediening" then
                                                listItemText.secondary (
                                                    $"**{s2}**"
                                                    |> Components.Markdown.render
                                                )
                                            else
                                                listItemText.secondary s2
                                        ]
                                    ]
                                    Mui.divider []
                                ]
                        )
                        |> Mui.list
                    | _ ->
                        [
                            "No valid content" |> Components.Markdown.render
                        ]
                        |> React.fragment

            Mui.dialog [
                dialog.open' (state.Dialog |> List.isEmpty |> not)
                dialog.children [
                    Mui.dialogTitle "Details"
                    Mui.dialogContent [
                        prop.style [ style.padding 40 ]
                        prop.children content
                    ]
                    Mui.dialogActions [
                        Mui.button [
                            prop.onClick (fun _ -> CloseDialog |> dispatch)
                            prop.text "OK"
                        ]
                    ]
                ]
                dialog.onClose (fun _ -> CloseDialog |> dispatch)
            ]

        Html.div [
            prop.id "lifesupport-div"
            prop.children [ emergencyList; dialog ]
        ]


    let render interventions =
        View({| interventions = interventions |})