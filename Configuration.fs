module Configuration

open System
open System.IO
open System.Drawing
open System.Windows.Forms
open Newtonsoft.Json

type Config =
    {
        MaxBalls  : int

        BallWidth  : int
        BallHeight  : int
        BallBoundingRadius  : int
        BallMaxConnections  : int

        BallMinVelocityX : int
        BallMaxVelocityX  : int

        BallMinVelocityY : int
        BallMaxVelocityY  : int

        BallConnectionThickness  : float32
    }
    with
        static member Default =
            {
                MaxBalls = 10
                BallWidth = 5
                BallHeight = 5
                BallBoundingRadius = 5
                BallMaxConnections = 5
                BallMinVelocityX = -1
                BallMaxVelocityX = 1
                BallMinVelocityY = -1
                BallMaxVelocityY = 1
                BallConnectionThickness = 2.0f
            }

let configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FancyScreensaver.json")

let loadConfig() =
    configPath
    |> File.ReadAllText
    |> JsonConvert.DeserializeObject<Config>

let writeConfig config =
    File.WriteAllText(configPath, JsonConvert.SerializeObject(config))

if configPath |> File.Exists |> not then
    writeConfig Config.Default

let configure() =
    let current = loadConfig()

    use configForm = new Form()

    let layout = new FlowLayoutPanel(AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Dock = DockStyle.Fill, Width = configForm.Width)

    layout.Controls.Add(new Label(Text="MaxBalls", AutoSize = true))
    layout.Controls.Add(new NumericUpDown(Maximum = 500.0M, DecimalPlaces = 0, Name="MaxBalls", Value = decimal current.MaxBalls))

    layout.Controls.Add(new Label(Text="BallWidth", AutoSize = true))
    layout.Controls.Add(new NumericUpDown(Maximum = 50.0M, DecimalPlaces = 0, Name="BallWidth", Value = decimal current.BallWidth))

    layout.Controls.Add(new Label(Text="BallHeight", AutoSize = true))
    layout.Controls.Add(new NumericUpDown(Maximum = 50.0M, DecimalPlaces = 0, Name="BallHeight", Value = decimal current.BallHeight))

    layout.Controls.Add(new Label(Text="BallBoundingRadius", AutoSize = true))
    layout.Controls.Add(new NumericUpDown(Maximum = 500.0M, DecimalPlaces = 0, Name="BallBoundingRadius", Value = decimal current.BallBoundingRadius))

    layout.Controls.Add(new Label(Text="BallMaxConnections", AutoSize = true))
    layout.Controls.Add(new NumericUpDown(Maximum = 50.0M, DecimalPlaces = 0, Name="BallMaxConnections", Value = decimal current.BallMaxConnections))

    layout.Controls.Add(new Label(Text="BallMinVelocityX", AutoSize = true))
    layout.Controls.Add(new NumericUpDown(Maximum = 50.0M, Minimum = -50.0M, DecimalPlaces = 0, Name="BallMinVelocityX", Value = decimal current.BallMinVelocityX))

    layout.Controls.Add(new Label(Text="BallMaxVelocityX", AutoSize = true))
    layout.Controls.Add(new NumericUpDown(Maximum = 50.0M, Minimum = -50.0M, DecimalPlaces = 0, Name="BallMaxVelocityX", Value = decimal current.BallMaxVelocityX))

    layout.Controls.Add(new Label(Text="BallMinVelocityY", AutoSize = true))
    layout.Controls.Add(new NumericUpDown(Maximum = 50.0M, Minimum = -50.0M, DecimalPlaces = 0, Name="BallMinVelocityY", Value = decimal current.BallMinVelocityY))

    layout.Controls.Add(new Label(Text="BallMaxVelocityY", AutoSize = true))
    layout.Controls.Add(new NumericUpDown(Maximum = 50.0M, Minimum = -50.0M, DecimalPlaces = 0, Name="BallMaxVelocityY", Value = decimal current.BallMaxVelocityY))

    layout.Controls.Add(new Label(Text="BallConnectionThickness", AutoSize = true))
    layout.Controls.Add(new NumericUpDown(Maximum = 50.0M, DecimalPlaces = 1, Name="BallConnectionThickness", Value = decimal current.BallConnectionThickness))

    layout.Controls.Add(new Button(Text = "Submit Changes", AutoSize = true, DialogResult = DialogResult.OK ))

    configForm.Controls.Add(layout)
    configForm.BringToFront()

    configForm.ShowDialog()
    |> ignore

    {
        MaxBalls =           int <| (layout.Controls.Find("MaxBalls", false).[0] :?> NumericUpDown).Value
        BallWidth =          int <| (layout.Controls.Find("BallWidth", false).[0] :?> NumericUpDown).Value
        BallHeight =         int <| (layout.Controls.Find("BallHeight", false).[0] :?> NumericUpDown).Value
        BallBoundingRadius = int <| (layout.Controls.Find("BallBoundingRadius", false).[0] :?> NumericUpDown).Value
        BallMaxConnections = int <| (layout.Controls.Find("BallMaxConnections", false).[0] :?> NumericUpDown).Value
        BallMinVelocityX =   int <| (layout.Controls.Find("BallMinVelocityX", false).[0] :?> NumericUpDown).Value
        BallMaxVelocityX =   int <| (layout.Controls.Find("BallMaxVelocityX", false).[0] :?> NumericUpDown).Value
        BallMinVelocityY =   int <| (layout.Controls.Find("BallMinVelocityY", false).[0] :?> NumericUpDown).Value
        BallMaxVelocityY =   int <| (layout.Controls.Find("BallMaxVelocityY", false).[0] :?> NumericUpDown).Value
        BallConnectionThickness = float32 <| (layout.Controls.Find("BallConnectionThickness", false).[0] :?> NumericUpDown).Value
    }
    |> writeConfig
