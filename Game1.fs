namespace FlappyBird

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

// Define the states the game can be in
type GameState =
    | Start
    | Playing
    | GameOver

type Game1() as this =
    inherit Game()

    // Initialize graphics and textures
    let graphics = new GraphicsDeviceManager(this)
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>
    let mutable birdUpflapTexture = Unchecked.defaultof<Texture2D>
    let mutable birdMidflapTexture = Unchecked.defaultof<Texture2D>
    let mutable birdDownflapTexture = Unchecked.defaultof<Texture2D>
    let mutable pipeTexture = Unchecked.defaultof<Texture2D>
    let mutable backgroundTexture = Unchecked.defaultof<Texture2D>
    let mutable baseTexture = Unchecked.defaultof<Texture2D>
    let mutable gameOverTexture = Unchecked.defaultof<Texture2D>
    let mutable messageTexture = Unchecked.defaultof<Texture2D>
    let mutable zero = Unchecked.defaultof<Texture2D>
    let mutable one = Unchecked.defaultof<Texture2D>
    let mutable two = Unchecked.defaultof<Texture2D>
    let mutable three = Unchecked.defaultof<Texture2D>
    let mutable four = Unchecked.defaultof<Texture2D>
    let mutable five = Unchecked.defaultof<Texture2D>
    let mutable six = Unchecked.defaultof<Texture2D>
    let mutable seven = Unchecked.defaultof<Texture2D>
    let mutable eight = Unchecked.defaultof<Texture2D>
    let mutable nine = Unchecked.defaultof<Texture2D>

    // Initialize game variables
    let mutable birdPosition = Vector2(100.0f, 300.0f)
    let mutable birdVelocity = Vector2.Zero
    let gravity = Vector2(0.0f, 0.5f)
    let jumpStrength = Vector2(0.0f, -5.0f)
    let pipeSpacing = 170.0f
    let pipeSpeed = 2.0f
    let pipeWidth = 50.0f
    let pipeGap = 120.0f
    let birdUpperLimit = 1.0f

    // Initialize pipe list
    let mutable pipes =
        List.empty<Vector2 * float32 * Vector2 * float32 * float32 * float32 list>

    // Initialize game state variables
    let mutable isGameOver = false
    let mutable gameState = GameState.Start
    let random = new Random()
    let mutable score = 0
    let mutable passedPipes = Set.empty<float32>

    let keyboardState = Keyboard.GetState()


    // Variables for bird animation
    let mutable birdFrame = 0
    let birdFrameCount = 3
    let birdFrameTime = 0.1f
    let mutable birdFrameTimer = 0.0f

    // Set up the game window
    do
        this.Content.RootDirectory <- "Content"
        graphics.PreferredBackBufferHeight <- 500
        graphics.PreferredBackBufferWidth <- 800
        graphics.ApplyChanges()

    // Function to create a pair of pipes
    let createPipePair (pipePositionX) =
        let viewportHeight = float32 this.GraphicsDevice.Viewport.Height
        let groundHeight = float32 baseTexture.Height

        let bottomPipePosition = viewportHeight - groundHeight
        let topPipePosition = 0.0f

        let bottomPipeHeight =
            float32 (random.Next(25, int (viewportHeight - groundHeight - pipeGap)))

        let topPipeHeight: float32 =
            viewportHeight - bottomPipeHeight - pipeGap - groundHeight

        let pipeGapPositionY =
            [ bottomPipeHeight + groundHeight; bottomPipeHeight + pipeGap + groundHeight ]

        let bottomPipe = Vector2(pipePositionX, bottomPipePosition - bottomPipeHeight)
        let topPipe = Vector2(pipePositionX, topPipePosition)

        (topPipe, topPipeHeight, bottomPipe, bottomPipeHeight, pipePositionX, pipeGapPositionY)

    // Initialize the game
    override _.Initialize() =
        base.Initialize()
        ()

    // Load game content (textures, etc.)
    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)
        birdUpflapTexture <- this.Content.Load<Texture2D>("png/bird-upflap")
        birdMidflapTexture <- this.Content.Load<Texture2D>("png/bird-midflap")
        birdDownflapTexture <- this.Content.Load<Texture2D>("png/bird-downflap")
        pipeTexture <- this.Content.Load<Texture2D>("png/pipe-green")
        backgroundTexture <- this.Content.Load<Texture2D>("png/background")
        baseTexture <- this.Content.Load<Texture2D>("png/base")
        gameOverTexture <- this.Content.Load<Texture2D>("png/gameover")
        messageTexture <- this.Content.Load<Texture2D>("png/message")
        zero <- this.Content.Load<Texture2D>("png/0")
        one <- this.Content.Load<Texture2D>("png/1")
        two <- this.Content.Load<Texture2D>("png/2")
        three <- this.Content.Load<Texture2D>("png/3")
        four <- this.Content.Load<Texture2D>("png/4")
        five <- this.Content.Load<Texture2D>("png/5")
        six <- this.Content.Load<Texture2D>("png/6")
        seven <- this.Content.Load<Texture2D>("png/7")
        eight <- this.Content.Load<Texture2D>("png/8")
        nine <- this.Content.Load<Texture2D>("png/9")

        pipes <- List.init 10 (fun i -> createPipePair (600.0f + (float32 i * pipeSpacing)))

    // Update game logic
    override this.Update(gameTime: GameTime) =
        if
            GamePad.GetState(PlayerIndex.One).Buttons.Back = ButtonState.Pressed
            || Keyboard.GetState().IsKeyDown(Keys.Escape)
        then
            this.Exit()

        let keyboardState = Keyboard.GetState()

        match gameState with
        | Start ->
            if keyboardState.IsKeyDown(Keys.Space) then
                gameState <- Playing

        | Playing ->
            if not isGameOver then
                if Keyboard.GetState().IsKeyDown(Keys.Space) then
                    birdVelocity <- jumpStrength

                birdVelocity <- birdVelocity + gravity
                birdPosition <- birdPosition + birdVelocity

                // Ensure the bird does not go above the upper limit
                if birdPosition.Y < birdUpperLimit then
                    birdPosition <- Vector2(birdPosition.X, birdUpperLimit)

                // Update bird animation
                birdFrameTimer <- birdFrameTimer + float32 gameTime.ElapsedGameTime.TotalSeconds

                if birdFrameTimer > birdFrameTime then
                    birdFrameTimer <- birdFrameTimer - birdFrameTime
                    birdFrame <- (birdFrame + 1) % birdFrameCount

                // Update pipes
                pipes <-
                    pipes
                    |> List.map
                        (fun (topPipe, topPipeHeight, bottomPipe, bottomPipeHeight, pipePosition, pipeGapPositionXY) ->

                            // Add logging before updating
                            let newTopPipe = topPipe - Vector2(pipeSpeed, 0.0f)
                            let newBottomPipe = bottomPipe - Vector2(pipeSpeed, 0.0f)
                            let newPipePosition = pipePosition - pipeSpeed

                            (newTopPipe,
                             topPipeHeight,
                             newBottomPipe,
                             bottomPipeHeight,
                             newPipePosition,
                             pipeGapPositionXY))

                    |> List.filter (fun (_, _, _, _, pipePosition, _) -> pipePosition > -pipeWidth)

                // Add new pipes if needed
                if pipes.Length < 10 then
                    let lastPipePosition =
                        pipes
                        |> List.maxBy (fun (_, _, _, _, pipePosition, _) -> pipePosition)
                        |> fun (_, _, _, _, pipePosition, _) -> pipePosition

                    pipes <- pipes @ [ createPipePair (lastPipePosition + pipeSpacing) ]

                // Check for collisions
                let birdRect =
                    Rectangle(int birdPosition.X, int birdPosition.Y, birdUpflapTexture.Width, birdUpflapTexture.Height)

                let pipeCollision =
                    pipes
                    |> List.exists (fun (topPipe, topPipeHeight, bottomPipe, bottomPipeHeight, _, _) ->
                        let topPipeRect =
                            Rectangle(int topPipe.X, int topPipe.Y, pipeTexture.Width, int topPipeHeight)

                        let bottomPipeRect =
                            Rectangle(int bottomPipe.X, int bottomPipe.Y, pipeTexture.Width, int bottomPipeHeight)

                        birdRect.Intersects(topPipeRect) || birdRect.Intersects(bottomPipeRect))

                if pipeCollision then
                    isGameOver <- true
                    gameState <- GameState.GameOver

                // Check if bird hits the ground
                if birdPosition.Y > float32 (this.GraphicsDevice.Viewport.Height - baseTexture.Height) then
                    isGameOver <- true
                    gameState <- GameState.GameOver

                let tolerance = 1.0f

                // Update score logic
                pipes
                |> List.iter (fun (_, _, _, _, pipePositionX, _) ->
                    let birdX = birdPosition.X

                    // Check if the bird has fully passed the trailing edge of the pipe
                    if birdX > pipePositionX + pipeWidth + tolerance then
                        // Ensure the pipe has not been passed already
                        if not (Set.contains pipePositionX passedPipes) then
                            // Mark all positions up to the next pipe as passed
                            let positionsToMark =
                                [ pipePositionX - pipeSpacing .. pipePositionX ]
                                |> List.map (fun pos -> pos - pipeSpeed)

                            // Add the pipe's position to the set passedPipes
                            passedPipes <- Set.union passedPipes (Set.ofList positionsToMark)
                            score <- score + 1
                    else
                        // Remove the pipe's position from the set if the bird has not passed it
                        passedPipes <- Set.remove pipePositionX passedPipes)

        | GameOver ->
            if keyboardState.IsKeyDown(Keys.R) then
                birdPosition <- Vector2(100.0f, 300.0f)
                birdVelocity <- Vector2.Zero
                isGameOver <- false
                gameState <- Start
                score <- 0
                passedPipes <- Set.empty
                pipes <- List.init 10 (fun i -> createPipePair (600.0f + (float32 i * pipeSpacing)))
            elif keyboardState.IsKeyDown(Keys.Escape) then
                this.Exit()
                base.Update(gameTime)

    // Draw game elements
    override this.Draw(gameTime: GameTime) =
        this.GraphicsDevice.Clear(Color.CornflowerBlue)

        spriteBatch.Begin()

        // Draw background
        for i in 0 .. (this.GraphicsDevice.Viewport.Width / backgroundTexture.Width) do
            spriteBatch.Draw(backgroundTexture, Vector2(float32 (i * backgroundTexture.Width), 0.0f), Color.White)

        match gameState with
        | Start ->
            // Draw message
            spriteBatch.Draw(
                messageTexture,
                Vector2(
                    float32 (this.GraphicsDevice.Viewport.Width / 2 - messageTexture.Width / 2),
                    float32 (this.GraphicsDevice.Viewport.Height / 2 - messageTexture.Height / 2)
                ),
                Color.White
            )

        | Playing ->
            // Draw ground
            for i in 0 .. (this.GraphicsDevice.Viewport.Width / baseTexture.Width) do
                spriteBatch.Draw(
                    baseTexture,
                    Vector2(
                        float32 (i * baseTexture.Width),
                        float32 (this.GraphicsDevice.Viewport.Height - baseTexture.Height)
                    ),
                    Color.White
                )

            // Draw bird animation
            let birdTexture =
                match birdFrame with
                | 0 -> birdUpflapTexture
                | 1 -> birdMidflapTexture
                | 2 -> birdDownflapTexture
                | _ -> birdUpflapTexture

            spriteBatch.Draw(birdTexture, birdPosition, Color.White)

            // Draw pipes
            for (topPipe, topPipeHeight, bottomPipe, bottomPipeHeight, _, _) in pipes do
                spriteBatch.Draw(
                    pipeTexture,
                    bottomPipe,
                    Nullable<Rectangle>(Rectangle(0, 0, pipeTexture.Width, int bottomPipeHeight)),
                    Color.White
                )

                spriteBatch.Draw(
                    pipeTexture,
                    topPipe,
                    Nullable<Rectangle>(Rectangle(0, 0, pipeTexture.Width, int topPipeHeight)),
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    1.0f,
                    SpriteEffects.FlipVertically,
                    0.0f
                )

            // Draw score
            let scoreString = score.ToString("D2")
            let scoreWidth = scoreString.Length * zero.Width

            let scorePosition =
                Vector2((float32 this.GraphicsDevice.Viewport.Width - float32 scoreWidth) / 2.0f, 50.0f)

            for i in 0 .. scoreString.Length - 1 do
                let digit = int (scoreString.[i].ToString())

                let texture =
                    match digit with
                    | 0 -> zero
                    | 1 -> one
                    | 2 -> two
                    | 3 -> three
                    | 4 -> four
                    | 5 -> five
                    | 6 -> six
                    | 7 -> seven
                    | 8 -> eight
                    | 9 -> nine
                    | _ -> zero

                spriteBatch.Draw(texture, scorePosition + Vector2(float32 (i * texture.Width), 0.0f), Color.White)

            // Draw game over screen if game is over
            if isGameOver then
                spriteBatch.Draw(
                    gameOverTexture,
                    Vector2(
                        float32 (this.GraphicsDevice.Viewport.Width / 2 - gameOverTexture.Width / 2),
                        float32 (this.GraphicsDevice.Viewport.Height / 2 - gameOverTexture.Height / 2)
                    ),
                    Color.White
                )

                // Draw restart prompt if game is over
                let restartMessage = "Press 'R' to Restart or 'Esc' to Exit"
                let font = this.Content.Load<SpriteFont>("font")
                let textSize = font.MeasureString(restartMessage)

                spriteBatch.DrawString(
                    font,
                    restartMessage,
                    Vector2(
                        float32 (this.GraphicsDevice.Viewport.Width / 2) - textSize.X / 2.0f,
                        float32 (this.GraphicsDevice.Viewport.Height / 2)
                        + float32 gameOverTexture.Height
                        + 20.0f
                    ),
                    Color.Orange
                )

        | GameOver ->
            // Draw game over screen
            spriteBatch.Draw(
                gameOverTexture,
                Vector2(
                    float32 (this.GraphicsDevice.Viewport.Width / 2 - gameOverTexture.Width / 2),
                    float32 (this.GraphicsDevice.Viewport.Height / 2 - gameOverTexture.Height / 2)
                ),
                Color.White
            )
            // Draw final score
            let scoreString = score.ToString("D2")
            let scoreWidth = scoreString.Length * zero.Width

            let scorePosition =
                Vector2((float32 this.GraphicsDevice.Viewport.Width - float32 scoreWidth) / 2.0f, 300.0f)

            for i in 0 .. scoreString.Length - 1 do
                let digit = int (scoreString.[i].ToString())

                let texture =
                    match digit with
                    | 0 -> zero
                    | 1 -> one
                    | 2 -> two
                    | 3 -> three
                    | 4 -> four
                    | 5 -> five
                    | 6 -> six
                    | 7 -> seven
                    | 8 -> eight
                    | 9 -> nine
                    | _ -> zero

                spriteBatch.Draw(texture, scorePosition + Vector2(float32 (i * texture.Width), 0.0f), Color.White)

            // Draw restart prompt
            let restartMessage = "Press 'R' to Restart or 'Esc' to Exit"
            let font = this.Content.Load<SpriteFont>("font")
            let textSize = font.MeasureString(restartMessage)

            spriteBatch.DrawString(
                font,
                restartMessage,
                Vector2(float32 (this.GraphicsDevice.Viewport.Width / 2) - textSize.X / 2.0f, 150.0f),
                Color.Orange
            )

        spriteBatch.End()

        base.Draw(gameTime)
