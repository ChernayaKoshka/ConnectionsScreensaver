open System
open System.Drawing
open System.Drawing.Drawing2D
open System.Windows.Forms
open System.Threading

let CANVAS_WIDTH = SystemInformation.VirtualScreen.Width
let CANVAS_HEIGHT = SystemInformation.VirtualScreen.Height

let config = Configuration.loadConfig()

type System.Random with
    member this.Next(minValue, maxValue, excludedValues) =
        let rec getNext() =
            let next = this.Next(minValue, maxValue)
            if excludedValues |> Seq.contains next then
                getNext()
            else
                next
        getNext()

let rand = Random()

type Ball =
    {
        Id : Guid
        Position : Point
        Velocity : Point
    }
    with
        member this.BoundingRect =
            Rectangle(this.Position.X, this.Position.Y, config.BallWidth, config.BallHeight)
        member this.BoundedRadius =
            let r = this.BoundingRect
            Rectangle(this.Position.X - config.BallBoundingRadius / 2, this.Position.Y - config.BallBoundingRadius / 2, r.Width + config.BallBoundingRadius, r.Height + config.BallBoundingRadius)

let ballPen = new Pen(Color.LightBlue, config.BallConnectionThickness)
let ballBrush = new SolidBrush(Color.LightGray)

let moveBall (bounds : Rectangle) (ball : Ball) =
    let boundingRect = ball.BoundingRect
    let (nextXPos, nextXVel) =
        if ball.Position.X + ball.Velocity.X + boundingRect.Width >= bounds.X + bounds.Width then
            (bounds.X + bounds.Width - boundingRect.Width, -ball.Velocity.X - rand.Next(-1, 1))
        else if ball.Position.X + ball.Velocity.X <= bounds.X then
            (bounds.X, -ball.Velocity.X - rand.Next(-1, 1))
        else
            (ball.Position.X + ball.Velocity.X, ball.Velocity.X)

    let (nextYPos, nextYVel) =
        if ball.Position.Y + ball.Velocity.Y + boundingRect.Height>= bounds.Y + bounds.Height then
            (bounds.Y + bounds.Height - boundingRect.Height, -ball.Velocity.Y - rand.Next(-1, 1))
        else if ball.Position.Y + ball.Velocity.Y <= bounds.Y then
            (bounds.Y, -ball.Velocity.Y - rand.Next(-1, 1))
        else
            (ball.Position.Y + ball.Velocity.Y, ball.Velocity.Y)

    { ball with Position = Point(nextXPos, nextYPos); Velocity = Point(nextXVel, nextYVel) }

type CanvasForm() as this =
    inherit Form()

    do this.SetStyle(ControlStyles.OptimizedDoubleBuffer ||| ControlStyles.AllPaintingInWmPaint ||| ControlStyles.UserPaint, true)

[<System.Runtime.InteropServices.DllImport("user32.dll")>]
extern bool ShowWindow(nativeint hWnd, int flags)
let HideConsole() =
    let proc = System.Diagnostics.Process.GetCurrentProcess()
    ShowWindow(proc.MainWindowHandle, 0)

[<EntryPoint>]
let main argv =
    HideConsole()
    |> ignore

    match argv with
    | [| |] | [| "/c" |] ->
        Configuration.configure()
        |> ignore
    | [| "/p" |] ->
        ()
    | [| "/s" |] | _ ->
        let startingCursorPos = Cursor.Position

        let t = Thread(ThreadStart(fun _ ->
            let canvas = new CanvasForm()
            canvas.SetDesktopLocation(0, 0)
            canvas.Width <- CANVAS_WIDTH;
            canvas.Height <- CANVAS_HEIGHT;
            canvas.BackColor <- Color.Black

            canvas.FormBorderStyle <- FormBorderStyle.None

            canvas.TopMost <- true

            let balls =
                [
                    for x = 0 to config.MaxBalls - 1 do
                        yield
                            {
                                Id = Guid.NewGuid()
                                Position = Point(rand.Next(canvas.Width), rand.Next(canvas.Height))
                                Velocity = Point(rand.Next(config.BallMinVelocityX, config.BallMaxVelocityX, [ 0 ]), rand.Next(config.BallMinVelocityY, config.BallMaxVelocityY, [ 0 ]))
                            }
                ]

            let current = ref balls

            canvas.Paint
            |> Event.add (fun p ->
                p.Graphics.SmoothingMode <- SmoothingMode.AntiAlias

                current.contents
                |> Seq.iter (fun nextBall ->
                    let rect = nextBall.BoundingRect

                    current.contents
                    |> Seq.filter (fun ball ->
                        nextBall.BoundedRadius.IntersectsWith(ball.BoundedRadius))
                    |> Seq.truncate config.BallMaxConnections
                    |> Seq.iter (fun ball ->
                        let ballRect = ball.BoundingRect
                        if nextBall.BoundedRadius.IntersectsWith(ball.BoundedRadius) then
                            p.Graphics.DrawLine(
                                ballPen,
                                nextBall.Position.X + (rect.Width / 2),
                                nextBall.Position.Y + (rect.Height / 2),
                                ball.Position.X + (ballRect.Width / 2),
                                ball.Position.Y + (ballRect.Height / 2))))

                current.contents
                |> Seq.iter (fun nextBall ->
                    p.Graphics.FillEllipse(ballBrush, nextBall.BoundingRect)))

            let ticker = async {
                let rec drawNext (balls : Ball list) =
                    if Math.Abs(Cursor.Position.X - startingCursorPos.X) > 5 || Math.Abs(Cursor.Position.Y - startingCursorPos.Y) > 5 then
                        Application.Exit()
                        ()
                    else
                        current := balls
                        canvas.Invalidate()
                        Thread.Sleep(50)
                        balls
                        |> List.map (fun ball ->
                            moveBall canvas.DisplayRectangle ball)
                        |> drawNext
                drawNext balls
            }

            Async.Start ticker

            canvas.Show()
            Cursor.Hide()
            Application.Run()))
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
    0 // return an integer exit code
