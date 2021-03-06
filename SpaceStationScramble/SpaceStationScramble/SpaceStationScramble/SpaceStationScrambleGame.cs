using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace SpaceStationScramble {
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SpaceStationScrambleGame : Microsoft.Xna.Framework.Game {
        const int SCREEN_WIDTH = 1280;
        const int SCREEN_HEIGHT = 720;

        //Input
        private KeyboardState currentKeyboardState;
        private KeyboardState previousKeyboardState;
        private GamePadState currentGamepadState;
        private GamePadState previousGamepadState;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Synchronizer synchronizer;
        string keyCode;
        double deathTime;

        PlayerNumber currentPlayer;
        MenuItem currentMenuItem;

        //Textures
        Texture2D titleScreenMenu;
        Texture2D instructions;
        Texture2D credits;
        Texture2D characterSelectionBackground;
        Texture2D[] menuSelector;
        Texture2D keyCodeBackground;
        Texture2D readyStartBackground;
        Texture2D playerOneBackground;
        Texture2D playerTwoBackground;
        Texture2D valvePanel;
        Texture2D displayTexture;

        SpriteFont font;

        Vector2 playerOneMenuPosition = new Vector2(475, 245);
        Vector2 playerTwoMenuPosition = new Vector2(475, 365);

        Vector2 firstMainMenuPosition = new Vector2(517, 217);
        Vector2 secondMainMenuPosition = new Vector2(517, 334);
        Vector2 thirdMainMenuPosition = new Vector2(517, 453);
        Vector2 fourthMainMenuPosition = new Vector2(517, 570);

        //Station info
        Dictionary<SpaceStationSection, Vector2> insideNodePositions;

        ScreenContext context = ScreenContext.TITLE_SCREEN_MENU;

        //Player One info
        Texture2D playerOneSprite; //For animation this will need to updated
        Vector2 playerOnePosition;
        Vector2 playerOneOffset;
        float playerOneMoveSpeed;
        Vector2 playerOneMoveStep;
        PlayerOneState playerOneState;

        //Player Two info
        Texture2D playerTwoSprite; //For animation this will need to updated
        Vector2 playerTwoPosition;
        float playerTwoMoveSpeed;

        //Disaster event info
        //eww eww eww, gross, gross, gross
        public static Texture2D steamTexture;
        public static Texture2D alarmTexture;
        public static Texture2D circleTexture;

        Texture2D satelliteDishTexture;
        Texture2D hatchTexture;
        Texture2D pipeTexture;
        Texture2D tankTexture;

        EventGenerator eventGenerator;
        DisasterEvent nextEvent;
        List<DisasterEvent> disasterEvents;
        double elapsedRoundTime;
        private GasValve currentlyClosingValve;
        private double valveClosingEndTime;

        private double repairEndTime;
        private bool repairing;

        SoundEffect valveTurn1;
        SoundEffect valveTurn2;
        SoundEffectInstance valve;
        bool valveSwap;

        SoundEffect affirmativeFeedback;
        SoundEffect negativeFeedback;
        SoundEffect groanAndExplosion;

        SoundEffect titleMusic;
        SoundEffectInstance titleMusicInstance;

        SoundEffect musicLoop1;
        SoundEffect musicLoop2;
        SoundEffect musicLoop3;
        SoundEffectInstance musicLoopInstance;

        SoundEffect gasLeakSound1;
        SoundEffect gasLeakSound2;
        SoundEffect gasLeakSound3;

        SoundEffect selectorMove;
        SoundEffect selectorClick;

        SoundEffect jetSound1;
        SoundEffect jetSound2;
        SoundEffect jetSound3;
        SoundEffect jetSound4;
        SoundEffectInstance jetSoundInstance;

        SoundEffect alarmSound;

        int currentMusic;

        bool isPlayerTwoMoving = false;

        private IDictionary<DisasterEvent, SoundEffectInstance> disasterSounds = new Dictionary<DisasterEvent, SoundEffectInstance>();


        private readonly Vector2 northSatPos = new Vector2(590, 200);
        private readonly Vector2 northTankPos = new Vector2(690, 200);
        private readonly Vector2 northPipePos = new Vector2(590, 300);
        private readonly Vector2 northHatchPos = new Vector2(690, 300);

        private readonly Vector2 southSatPos = new Vector2(590, 550);
        private readonly Vector2 southTankPos = new Vector2(690, 550);
        private readonly Vector2 southPipePos = new Vector2(590, 660);
        private readonly Vector2 southHatchPos = new Vector2(690, 660);

        private readonly Vector2 eastSatPos = new Vector2(1090, 420);
        private readonly Vector2 eastTankPos = new Vector2(1000, 420);
        private readonly Vector2 eastPipePos = new Vector2(870, 380);
        private readonly Vector2 eastHatchPos = new Vector2(870, 445);

        private readonly Vector2 westSatPos = new Vector2(200, 420);
        private readonly Vector2 westTankPos = new Vector2(290, 420);
        private readonly Vector2 westPipePos = new Vector2(400, 380);
        private readonly Vector2 westHatchPos = new Vector2(400, 445);

        private readonly Vector2 centerSatPos = new Vector2(590, 390);
        private readonly Vector2 centerTankPos = new Vector2(680, 450);
        private readonly Vector2 centerPipePos = new Vector2(590, 450);
        private readonly Vector2 centerHatchPos = new Vector2(680, 390);

        private List<RepairPart> repairableParts;
        private RepairPart currentRepairPart;

        public SpaceStationScrambleGame() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            synchronizer = new Synchronizer();

            graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
            graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            // TODO: Add your initialization logic here

            //Start with player one for now
            currentPlayer = PlayerNumber.ONE;
            currentMenuItem = MenuItem.NEW_GAME;

            //Setup space station information
            insideNodePositions = new Dictionary<SpaceStationSection, Vector2>();
            insideNodePositions.Add(SpaceStationSection.CENTER, new Vector2(640, 360));
            insideNodePositions.Add(SpaceStationSection.NORTH, new Vector2(640, 60));
            insideNodePositions.Add(SpaceStationSection.SOUTH, new Vector2(640, 660));
            insideNodePositions.Add(SpaceStationSection.EAST, new Vector2(980, 360));
            insideNodePositions.Add(SpaceStationSection.WEST, new Vector2(300, 360));

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            titleScreenMenu = Content.Load<Texture2D>("gfx/title-screen-menu");
            instructions = Content.Load<Texture2D>("gfx/instructions");
            credits = Content.Load<Texture2D>("gfx/credits");
            characterSelectionBackground = Content.Load<Texture2D>("gfx/character-selection");
            menuSelector = new Texture2D[4];
            menuSelector[0] = Content.Load<Texture2D>("gfx/MenuSelect_01");
            menuSelector[1] = Content.Load<Texture2D>("gfx/MenuSelect_02");
            menuSelector[2] = Content.Load<Texture2D>("gfx/MenuSelect_04");
            menuSelector[3] = Content.Load<Texture2D>("gfx/MenuSelect_02");

            keyCodeBackground = Content.Load<Texture2D>("gfx/key-code-background");
            readyStartBackground = Content.Load<Texture2D>("gfx/ready-start");


            playerOneBackground = Content.Load<Texture2D>("gfx/ShipIndoors");
            playerTwoBackground = Content.Load<Texture2D>("gfx/outside-rough");

            playerOneSprite = Content.Load<Texture2D>("gfx/player");
            playerTwoSprite = Content.Load<Texture2D>("gfx/player");

            steamTexture = Content.Load<Texture2D>("gfx/steam");
            tankTexture = Content.Load<Texture2D>("gfx/O2Tank");
            hatchTexture = Content.Load<Texture2D>("gfx/hatch");
            satelliteDishTexture = Content.Load<Texture2D>("gfx/satellite");
            pipeTexture = Content.Load<Texture2D>("gfx/pipes");
            circleTexture = Content.Load<Texture2D>("gfx/circle");
            alarmTexture = Content.Load<Texture2D>("gfx/alarm");
            displayTexture = Content.Load<Texture2D>("gfx/Screen");

            font = Content.Load<SpriteFont>("font/Segoe UI Mono");

            valvePanel = Content.Load<Texture2D>("gfx/valve-panel");

            valveTurn1 = Content.Load<SoundEffect>("sound/Valve Turn 1");
            valveTurn2 = Content.Load<SoundEffect>("sound/Valve Turn 2");

            affirmativeFeedback = Content.Load<SoundEffect>("sound/Affirmative FB");
            negativeFeedback = Content.Load<SoundEffect>("sound/Negative FB");

            groanAndExplosion = Content.Load<SoundEffect>("sound/Ship Groan and Explosion 1");

            titleMusic = Content.Load<SoundEffect>("sound/Title Music - Loop");
            titleMusicInstance = titleMusic.CreateInstance();
            titleMusicInstance.IsLooped = true;
            titleMusicInstance.Play();

            musicLoop1 = Content.Load<SoundEffect>("sound/MUSIC LOOP 1");
            musicLoop2 = Content.Load<SoundEffect>("sound/MUSIC LOOP 2");
            musicLoop3 = Content.Load<SoundEffect>("sound/MUSIC LOOP 3");

            gasLeakSound1 = Content.Load<SoundEffect>("sound/Pressure Burst and Leak 1");
            gasLeakSound2 = Content.Load<SoundEffect>("sound/Pressure Burst and Leak 2");
            gasLeakSound3 = Content.Load<SoundEffect>("sound/Pressure Burst and Leak 3");

            selectorMove = Content.Load<SoundEffect>("sound/Start Screen Selector 1");
            selectorClick = Content.Load<SoundEffect>("sound/Start Screen Selection 1");

            jetSound1 = Content.Load<SoundEffect>("sound/Jet Sound 7 loong w pitch_07");
            jetSound2 = Content.Load<SoundEffect>("sound/Jet Sound 8");
            jetSound3 = Content.Load<SoundEffect>("sound/Jet Sound 9");
            jetSound4 = Content.Load<SoundEffect>("sound/Jet Sound 10");

            alarmSound = Content.Load<SoundEffect>("sound/Alarm 1");

            //Hatches
            repairableParts = new List<RepairPart>();
            repairableParts.Add(new RepairPart(false, StationPart.Hatch, EventSlot.North, northHatchPos, hatchTexture));
            repairableParts.Add(new RepairPart(false, StationPart.Hatch, EventSlot.South, southHatchPos, hatchTexture));
            repairableParts.Add(new RepairPart(false, StationPart.Hatch, EventSlot.East, eastHatchPos, hatchTexture));
            repairableParts.Add(new RepairPart(false, StationPart.Hatch, EventSlot.West, westHatchPos, hatchTexture));
            repairableParts.Add(new RepairPart(false, StationPart.Hatch, EventSlot.Center, centerHatchPos, hatchTexture));
            //Tanks
            repairableParts.Add(new RepairPart(false, StationPart.O2Tank, EventSlot.North, northTankPos, tankTexture));
            repairableParts.Add(new RepairPart(false, StationPart.O2Tank, EventSlot.South, southTankPos, tankTexture));
            repairableParts.Add(new RepairPart(false, StationPart.O2Tank, EventSlot.East, eastTankPos, tankTexture));
            repairableParts.Add(new RepairPart(false, StationPart.O2Tank, EventSlot.West, westTankPos, tankTexture));
            repairableParts.Add(new RepairPart(false, StationPart.O2Tank, EventSlot.Center, centerTankPos, tankTexture));
            //Pipes
            repairableParts.Add(new RepairPart(false, StationPart.Pipe, EventSlot.North, northPipePos, pipeTexture));
            repairableParts.Add(new RepairPart(false, StationPart.Pipe, EventSlot.South, southPipePos, pipeTexture));
            repairableParts.Add(new RepairPart(false, StationPart.Pipe, EventSlot.East, eastPipePos, pipeTexture));
            repairableParts.Add(new RepairPart(false, StationPart.Pipe, EventSlot.West, westPipePos, pipeTexture));
            repairableParts.Add(new RepairPart(false, StationPart.Pipe, EventSlot.Center, centerPipePos, pipeTexture));
            //Satellite Dishes
            repairableParts.Add(new RepairPart(false, StationPart.SatelliteDish, EventSlot.North, northSatPos, satelliteDishTexture));
            repairableParts.Add(new RepairPart(false, StationPart.SatelliteDish, EventSlot.South, southSatPos, satelliteDishTexture));
            repairableParts.Add(new RepairPart(false, StationPart.SatelliteDish, EventSlot.East, eastSatPos, satelliteDishTexture));
            repairableParts.Add(new RepairPart(false, StationPart.SatelliteDish, EventSlot.West, westSatPos, satelliteDishTexture));
            repairableParts.Add(new RepairPart(false, StationPart.SatelliteDish, EventSlot.Center, centerSatPos, satelliteDishTexture));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            previousGamepadState = currentGamepadState;
            currentGamepadState = GamePad.GetState(PlayerIndex.One);

            switch (context) {
                case ScreenContext.TITLE_SCREEN_MENU:
                    if (titleMusicInstance.Volume * 1.05f < 1) {
                        titleMusicInstance.Volume += 0.001f;
                        titleMusicInstance.Volume *= 1.05f;
                    }
                    if (musicLoopInstance != null) {
                        musicLoopInstance.Stop();
                    }
                    if (jetSoundInstance != null) {
                        jetSoundInstance.Stop();
                    }
                    if (isNewlyPressedUp()) {
                        if (currentMenuItem != MenuItem.NEW_GAME) {
                            currentMenuItem--;
                            selectorMove.Play();
                        }
                    }
                    if (isNewlyPressedDown()) {
                        if (currentMenuItem != MenuItem.EXIT) {
                            currentMenuItem++;
                            selectorMove.Play();
                        }
                    }
                    if (isNewlyPressedStart()) {
                        selectorClick.Play();
                        switch (currentMenuItem) {
                            case MenuItem.NEW_GAME:
                                context = ScreenContext.CHARACTER_SELECTION;
                                break;
                            case MenuItem.INSTRUCTIONS:
                                context = ScreenContext.INSTRUCTIONS;
                                break;
                            case MenuItem.CREDITS:
                                context = ScreenContext.CREDITS;
                                break;
                            case MenuItem.EXIT:
                                this.Exit();
                                break;
                        }
                    }
                    break;
                case ScreenContext.INSTRUCTIONS:
                case ScreenContext.CREDITS:
                case ScreenContext.DEATH:
                    if (musicLoopInstance != null) {
                        musicLoopInstance.Stop();
                    }
                    if (jetSoundInstance != null) {
                        jetSoundInstance.Stop();
                    }
                    if (titleMusicInstance.Volume * 1.05f < 1) {
                        titleMusicInstance.Volume += 0.001f;
                        titleMusicInstance.Volume *= 1.05f;
                    }
                    if (isNewlyPressedStart() || isNewlyPressedBack()) {
                        context = ScreenContext.TITLE_SCREEN_MENU;
                    }
                    break;
                case ScreenContext.CHARACTER_SELECTION:
                    if (isNewlyPressedUp()) {
                        if (currentPlayer == PlayerNumber.TWO) {
                            currentPlayer = PlayerNumber.ONE;
                            selectorMove.Play();
                        }
                    } else if (isNewlyPressedDown()) {
                        if (currentPlayer == PlayerNumber.ONE) {
                            currentPlayer = PlayerNumber.TWO;
                            selectorMove.Play();
                        }
                    }
                    if (isNewlyPressedStart()) {
                        if (currentPlayer == PlayerNumber.ONE) {
                            keyCode = synchronizer.GenerateKeyCode();
                        } else {
                            keyCode = string.Empty;
                        }
                        context = ScreenContext.KEY_CODE;
                        selectorClick.Play();
                    }
                    if (isNewlyPressedBack()) {
                        context = ScreenContext.TITLE_SCREEN_MENU;
                    }
                    break;
                case ScreenContext.KEY_CODE:
                    if (currentPlayer == PlayerNumber.ONE) {
                        if (isNewlyPressedStart()) {
                            context = ScreenContext.READY_TO_START;
                        }
                        if (isNewlyPressedBack()) {
                            context = ScreenContext.CHARACTER_SELECTION;
                        }
                    } else {
                        keyCode += getTypedCharacter();
                        if (isNewlyPressedStart()) {
                            try {
                                synchronizer.AcceptKeyCode(keyCode);
                                context = ScreenContext.READY_TO_START;
                            } catch (InvalidKeyCodeException) {
                                keyCode = string.Empty;
                            }
                        }
                        if (isNewlyPressedBack()) {
                            if (keyCode.Length > 0) {
                                keyCode = keyCode.Remove(keyCode.Length - 1);
                            } else {
                                context = ScreenContext.CHARACTER_SELECTION;
                            }
                        }
                    }
                    break;
                case ScreenContext.READY_TO_START:
                    if (titleMusicInstance.Volume > 0.01f) {
                        titleMusicInstance.Volume *= 0.95f;
                    } else {
                        titleMusicInstance.Volume = 0;
                    }
                    if (isNewlyPressedStart()) {
                        setInitialGameState();
                        context = ScreenContext.GAME_PLAY;
                    }
                    if (isNewlyPressedBack()) {
                        context = ScreenContext.TITLE_SCREEN_MENU;
                    }
                    break;
                case ScreenContext.GAME_PLAY:
                    if (titleMusicInstance.Volume > 0.01f) {
                        titleMusicInstance.Volume *= 0.95f;
                    } else {
                        titleMusicInstance.Volume = 0;
                    }
                    if (musicLoopInstance.State != SoundState.Playing) {
                        nextMusic();
                    }
                    // Allows the game to exit
                    if (currentGamepadState.Buttons.Back == ButtonState.Pressed
                            || currentKeyboardState.IsKeyDown(Keys.Escape)) {
                        context = ScreenContext.TITLE_SCREEN_MENU;
                        stopDisasterSounds();
                    }

                    elapsedRoundTime += gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (nextEvent.StartTime <= elapsedRoundTime) {
                        disasterEvents.Add(nextEvent);
                        if (nextEvent.VisibleToPlayer == currentPlayer) {
                            disasterSounds[nextEvent] = getDisasterSoundInstance(nextEvent);
                            disasterSounds[nextEvent].Play();
                        }
                        nextEvent = eventGenerator.NextEvent();
                    }

                    List<DisasterEvent> eventsToRemove = new List<DisasterEvent>();
                    foreach (DisasterEvent theEvent in disasterEvents) {
                        if (theEvent.EndTime <= elapsedRoundTime) {
                            eventsToRemove.Add(theEvent);
                            if (theEvent.VisibleToPlayer != currentPlayer) {
                                deathTime = elapsedRoundTime;
                                groanAndExplosion.Play();
                                stopDisasterSounds();
                                context = ScreenContext.DEATH;
                            } else {
                                disasterSounds[theEvent].Stop();
                                disasterSounds.Remove(theEvent);
                            }
                        } else {
                            theEvent.Update();
                        }
                    }

                    foreach (DisasterEvent theEvent in eventsToRemove) {
                        disasterEvents.Remove(theEvent);
                    }
                    eventsToRemove.Clear();

                    //Update player one
                    if (currentPlayer == PlayerNumber.ONE) {
                        InputKey pressedKey = InputKey.None;

                        if (currentGamepadState.IsButtonDown(Buttons.DPadUp) || currentKeyboardState.IsKeyDown(Keys.Up) || currentKeyboardState.IsKeyDown(Keys.W)) {
                            pressedKey = InputKey.MoveUp;
                        }
                        if (currentGamepadState.IsButtonDown(Buttons.DPadDown) || currentKeyboardState.IsKeyDown(Keys.Down) || currentKeyboardState.IsKeyDown(Keys.S)) {
                            pressedKey = InputKey.MoveDown;
                        }
                        if (currentGamepadState.IsButtonDown(Buttons.DPadRight) || currentKeyboardState.IsKeyDown(Keys.Right) || currentKeyboardState.IsKeyDown(Keys.D)) {
                            pressedKey = InputKey.MoveRight;
                        }
                        if (currentGamepadState.IsButtonDown(Buttons.DPadLeft) || currentKeyboardState.IsKeyDown(Keys.Left) || currentKeyboardState.IsKeyDown(Keys.A)) {
                            pressedKey = InputKey.MoveLeft;
                        }

                        processPlayerOneInput(pressedKey);
                        checkPlayerPosition();

                        //Update player1 position
                        playerOnePosition += playerOneMoveStep;

                        if (playerOneState == PlayerOneState.AtCenter
                                || playerOneState == PlayerOneState.AtEast
                                || playerOneState == PlayerOneState.AtNorth
                                || playerOneState == PlayerOneState.AtSouth
                                || playerOneState == PlayerOneState.AtWest) {
                            if (currentGamepadState.IsButtonDown(Buttons.X) || currentKeyboardState.IsKeyDown(Keys.D1)) {
                                beginClosingGasValve(GasValve.BLUE);
                            } else if (currentGamepadState.IsButtonDown(Buttons.Y) || currentKeyboardState.IsKeyDown(Keys.D2)) {
                                beginClosingGasValve(GasValve.YELLOW);
                            } else if (currentGamepadState.IsButtonDown(Buttons.A) || currentKeyboardState.IsKeyDown(Keys.D3)) {
                                beginClosingGasValve(GasValve.GREEN);
                            } else if (currentGamepadState.IsButtonDown(Buttons.B) || currentKeyboardState.IsKeyDown(Keys.D4)) {
                                beginClosingGasValve(GasValve.RED);
                            }
                        } else {
                            stopClosingValve();
                        }

                        if (currentlyClosingValve != GasValve.NONE && valveClosingEndTime < elapsedRoundTime) {
                            finishClosingGasValve(currentlyClosingValve);
                        }

                        clearAlarm();
                    } else {
                        bool wasPlayerTwoMoving = isPlayerTwoMoving;
                        isPlayerTwoMoving = false;

                        if (currentGamepadState.IsButtonDown(Buttons.DPadUp) || currentKeyboardState.IsKeyDown(Keys.Up) || currentKeyboardState.IsKeyDown(Keys.W)) {
                            playerTwoPosition.Y -= playerTwoMoveSpeed;
                            isPlayerTwoMoving = true;
                        }
                        if (currentGamepadState.IsButtonDown(Buttons.DPadDown) || currentKeyboardState.IsKeyDown(Keys.Down) || currentKeyboardState.IsKeyDown(Keys.S)) {
                            playerTwoPosition.Y += playerTwoMoveSpeed;
                            isPlayerTwoMoving = true;
                        }
                        if (currentGamepadState.IsButtonDown(Buttons.DPadRight) || currentKeyboardState.IsKeyDown(Keys.Right) || currentKeyboardState.IsKeyDown(Keys.D)) {
                            playerTwoPosition.X += playerTwoMoveSpeed;
                            isPlayerTwoMoving = true;
                        }
                        if (currentGamepadState.IsButtonDown(Buttons.DPadLeft) || currentKeyboardState.IsKeyDown(Keys.Left) || currentKeyboardState.IsKeyDown(Keys.A)) {
                            playerTwoPosition.X -= playerTwoMoveSpeed;
                            isPlayerTwoMoving = true;
                        }
                        if (isPlayerTwoMoving && !wasPlayerTwoMoving) {
                            if (jetSoundInstance != null) {
                                jetSoundInstance.Stop();
                            }
                            jetSoundInstance = getNextJetSoundInstance();
                            jetSoundInstance.Play();
                        } else if (!isPlayerTwoMoving && wasPlayerTwoMoving) {
                            jetSoundInstance.Stop();
                        }

                        //See if we're in range of any repairable parts
                        float repairRadius = 16.0f;
                        bool partActive = false;
                        repairing = false;
                        foreach (RepairPart part in repairableParts) {
                            if (Vector2.Distance(playerTwoPosition, part.Position) < repairRadius) {
                                if (part.Active) {
                                    partActive = true;
                                }
                                partActive = true;
                                switch (part.Part) {
                                    case StationPart.Hatch:
                                        if (currentGamepadState.IsButtonDown(Buttons.B)) {
                                            if (!part.Active) {
                                                startRepair(part);
                                                repairing = true;
                                                currentRepairPart = part;
                                            }
                                        }
                                        break;
                                    case StationPart.O2Tank:
                                        if (currentGamepadState.IsButtonDown(Buttons.X)) {
                                            if (!part.Active) {
                                                startRepair(part);
                                                repairing = true;
                                                currentRepairPart = part;
                                            }
                                        }
                                        break;
                                    case StationPart.Pipe:
                                        if (currentGamepadState.IsButtonDown(Buttons.A)) {
                                            if (!part.Active) {
                                                startRepair(part);
                                                repairing = true;
                                                currentRepairPart = part;
                                            }
                                        }
                                        break;
                                    case StationPart.SatelliteDish:
                                        if (currentGamepadState.IsButtonDown(Buttons.Y)) {
                                            if (!part.Active) {
                                                startRepair(part);
                                                repairing = true;
                                                currentRepairPart = part;
                                            }
                                        }
                                        break;
                                }
                            } else {
                                part.Active = false;
                            }
                        }

                        //Update the repair status
                        if (partActive) {
                            repairing = true;
                        }

                        updateRepairStatus();
                    }
                    break;
            }

            base.Update(gameTime);
        }

        private void clearAlarm() {
            foreach(DisasterEvent disaster in disasterEvents) {
                if (disaster is RepairDisaster && sameLocation(playerOneState, disaster.Slot)) {
                    disasterSounds[disaster].Volume = 0f;
                    ((RepairDisaster) disaster).IsSilenced = true;
                }
            }
        }

        private void stopDisasterSounds() {
            foreach (var element in disasterSounds) {
                if (element.Value != null) {
                    element.Value.Stop();
                }
            }
        }

        private int jetSoundRotator = 0;

        private SoundEffectInstance getNextJetSoundInstance() {
            jetSoundRotator++;
            switch (jetSoundRotator % 4) {
                case 0:
                    return jetSound1.CreateInstance();
                case 1:
                    return jetSound2.CreateInstance();
                case 2:
                    return jetSound3.CreateInstance();
                default:
                    return jetSound4.CreateInstance();
            }
        }

        private void startRepair(RepairPart part) {
            part.Active = true;
            repairEndTime = elapsedRoundTime + 2000;
        }

        private void updateRepairStatus() {
            if (repairEndTime <= elapsedRoundTime) {
                if (currentRepairPart != null && currentRepairPart.Active) {
                    var eventsToRemove = new List<DisasterEvent>();
                    foreach (DisasterEvent theEvent in disasterEvents) {
                        if (theEvent is RepairDisaster) {
                            RepairDisaster repairEvent = theEvent as RepairDisaster;
                            if (repairEvent.Slot == currentRepairPart.Slot
                                && repairEvent.StationPart == currentRepairPart.Part) {
                                    eventsToRemove.Add(theEvent);
                            }
                        }
                    }
                    if (eventsToRemove.Count > 0) {
                        affirmativeFeedback.Play();
                    } else {
                        negativeFeedback.Play();
                    }
                    currentRepairPart.Active = false;
                    disasterEvents = disasterEvents.Except(eventsToRemove).ToList();
                }
            }
        }

        private int gasLeakSoundRotator = 0;

        private SoundEffectInstance getDisasterSoundInstance(DisasterEvent nextEvent) {
            if (nextEvent is GasLeakDisaster) {
                gasLeakSoundRotator++;
                switch (gasLeakSoundRotator % 3) {
                    case 0:
                        return gasLeakSound1.CreateInstance();
                    case 1:
                        return gasLeakSound2.CreateInstance();
                    default:
                        return gasLeakSound3.CreateInstance();
                }
            } else {
                return alarmSound.CreateInstance();
            }
        }

        private void nextMusic() {
            currentMusic++;
            if (currentMusic >= 6) {
                currentMusic = 5;
            }
            switch (currentMusic) {
                case 0:
                case 1:
                    musicLoopInstance = musicLoop1.CreateInstance();
                    break;
                case 2:
                case 3:
                    musicLoopInstance = musicLoop2.CreateInstance();
                    break;
                case 4:
                case 5:
                    musicLoopInstance = musicLoop3.CreateInstance();
                    break;
            }
            musicLoopInstance.Volume = 0.5f;
            musicLoopInstance.Play();
        }

        private void setInitialGameState() {
            currentlyClosingValve = GasValve.NONE;
            valveClosingEndTime = Double.MaxValue;
            repairing = false;
            repairEndTime = Double.MaxValue;

            //initialize player one properties
            playerOneMoveSpeed = 3.0f;
            playerOneMoveStep = Vector2.Zero;

            playerOnePosition = insideNodePositions[SpaceStationSection.CENTER];
            playerOneState = PlayerOneState.AtCenter;

            //initialize player two properties
            playerTwoMoveSpeed = 3.0f;
            playerTwoPosition = new Vector2(640, 360);

            disasterEvents = new List<DisasterEvent>();
            eventGenerator = new EventGenerator(synchronizer);
            elapsedRoundTime = 0;
            nextEvent = eventGenerator.NextEvent();

            musicLoopInstance = musicLoop1.CreateInstance();
            musicLoopInstance.Volume = 0.5f;
            musicLoopInstance.Play();
            currentMusic = 0;
        }

        private void beginClosingGasValve(GasValve gasValve) {
            if (currentlyClosingValve != gasValve) {
                currentlyClosingValve = gasValve;
                valveClosingEndTime = elapsedRoundTime + 2000;
                valve = getValveInstance();
                valve.Play();
            }
        }

        private SoundEffectInstance getValveInstance() {
            valveSwap = !valveSwap;
            if (valveSwap) {
                return valveTurn1.CreateInstance();
            } else {
                return valveTurn2.CreateInstance();
            }
        }

        private void stopClosingValve() {
            currentlyClosingValve = GasValve.NONE;
            valveClosingEndTime = Double.MaxValue;
            if (valve != null) {
                valve.Stop();
            }
        }
        private void finishClosingGasValve(GasValve gasValve) {
            currentlyClosingValve = GasValve.NONE;
            valveClosingEndTime = Double.MaxValue;
            removePendingGasLeak(gasValve, playerOneState);
        }

        private void removePendingGasLeak(GasValve gasValve, PlayerOneState playerOneState) {
            var eventsToRemove = new List<DisasterEvent>();
            foreach (var disaster in disasterEvents) {
                if (disaster is GasLeakDisaster) {
                    GasLeakDisaster gld = (GasLeakDisaster)disaster;
                    if (sameColor(gasValve, gld.SteamColor) && sameLocation(playerOneState, gld.Slot)) {
                        eventsToRemove.Add(disaster);
                    }
                }
            }
            if (eventsToRemove.Count > 0) {
                affirmativeFeedback.Play();
            } else {
                negativeFeedback.Play();
            }
            disasterEvents = disasterEvents.Except(eventsToRemove).ToList();
        }

        private bool sameColor(GasValve gasValve, SteamColor steamColor) {
            if (gasValve == GasValve.BLUE && steamColor == SteamColor.Blue) {
                return true;
            } else if (gasValve == GasValve.YELLOW && steamColor == SteamColor.Yellow) {
                return true;
            } else if (gasValve == GasValve.GREEN && steamColor == SteamColor.Green) {
                return true;
            } else if (gasValve == GasValve.RED && steamColor == SteamColor.Red) {
                return true;
            } else {
                return false;
            }
        }

        private bool sameLocation(PlayerOneState playerOneState, EventSlot slot) {
            if (playerOneState == PlayerOneState.AtCenter && slot == EventSlot.Center) {
                return true;
            } else if (playerOneState == PlayerOneState.AtEast && slot == EventSlot.East) {
                return true;
            } else if (playerOneState == PlayerOneState.AtNorth && slot == EventSlot.North) {
                return true;
            } else if (playerOneState == PlayerOneState.AtSouth && slot == EventSlot.South) {
                return true;
            } else if (playerOneState == PlayerOneState.AtWest && slot == EventSlot.West) {
                return true;
            } else {
                return false;
            }
        }

        private string getTypedCharacter() {
            Keys[] pressedKeys = currentKeyboardState.GetPressedKeys();
            foreach (Keys key in pressedKeys) {
                if (previousKeyboardState.IsKeyUp(key)) {
                    string value = key.ToString();
                    if (value.Length == 1) {
                        if (value.CompareTo("A") >= 0 && value.CompareTo("Z") <= 0) {
                            if (currentKeyboardState.IsKeyDown(Keys.LeftShift) || currentKeyboardState.IsKeyDown(Keys.RightShift)) {
                                return value;
                            } else {
                                return value.ToLower();
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }

        private void processPlayerOneInput(InputKey key) {
            switch (playerOneState) {
                case PlayerOneState.AtCenter:
                    switch (key) {
                        case InputKey.MoveUp:
                            playerOneState = PlayerOneState.CenterToNorth;
                            playerOneMoveStep = new Vector2(0.0f, -playerOneMoveSpeed);
                            break;
                        case InputKey.MoveDown:
                            playerOneState = PlayerOneState.CenterToSouth;
                            playerOneMoveStep = new Vector2(0.0f, playerOneMoveSpeed);
                            break;
                        case InputKey.MoveLeft:
                            playerOneState = PlayerOneState.CenterToWest;
                            playerOneMoveStep = new Vector2(-playerOneMoveSpeed, 0.0f);
                            break;
                        case InputKey.MoveRight:
                            playerOneState = PlayerOneState.CenterToEast;
                            playerOneMoveStep = new Vector2(playerOneMoveSpeed, 0.0f);
                            break;
                    }
                    break;
                case PlayerOneState.CenterToNorth:
                case PlayerOneState.AtNorth:
                    if (key == InputKey.MoveDown) {
                        playerOneState = PlayerOneState.NorthToCenter;
                        playerOneMoveStep = new Vector2(0.0f, playerOneMoveSpeed);
                    }
                    break;
                case PlayerOneState.CenterToSouth:
                case PlayerOneState.AtSouth:
                    if (key == InputKey.MoveUp) {
                        playerOneState = PlayerOneState.SouthToCenter;
                        playerOneMoveStep = new Vector2(0.0f, -playerOneMoveSpeed);
                    }
                    break;
                case PlayerOneState.CenterToWest:
                case PlayerOneState.AtWest:
                    if (key == InputKey.MoveRight) {
                        playerOneState = PlayerOneState.WestToCenter;
                        playerOneMoveStep = new Vector2(playerOneMoveSpeed, 0.0f);
                    }
                    break;
                case PlayerOneState.CenterToEast:
                case PlayerOneState.AtEast:
                    if (key == InputKey.MoveLeft) {
                        playerOneState = PlayerOneState.EastToCenter;
                        playerOneMoveStep = new Vector2(-playerOneMoveSpeed, 0.0f);
                    }
                    break;
                case PlayerOneState.NorthToCenter:
                    if (key == InputKey.MoveUp) {
                        playerOneState = PlayerOneState.CenterToNorth;
                        playerOneMoveStep = new Vector2(0.0f, -playerOneMoveSpeed);
                    }
                    break;
                case PlayerOneState.SouthToCenter:
                    if (key == InputKey.MoveDown) {
                        playerOneState = PlayerOneState.CenterToSouth;
                        playerOneMoveStep = new Vector2(0.0f, playerOneMoveSpeed);
                    }
                    break;
                case PlayerOneState.EastToCenter:
                    if (key == InputKey.MoveRight) {
                        playerOneState = PlayerOneState.CenterToEast;
                        playerOneMoveStep = new Vector2(playerOneMoveSpeed, 0.0f);
                    }
                    break;
                case PlayerOneState.WestToCenter:
                    if (key == InputKey.MoveLeft) {
                        playerOneState = PlayerOneState.CenterToWest;
                        playerOneMoveStep = new Vector2(-playerOneMoveSpeed, 0.0f);
                    }
                    break;
            }
        }

        private void checkPlayerPosition() {
            switch (playerOneState) {
                case PlayerOneState.CenterToNorth:
                    if (Vector2.Distance(playerOnePosition, insideNodePositions[SpaceStationSection.NORTH]) < playerOneMoveSpeed) {
                        playerOnePosition = insideNodePositions[SpaceStationSection.NORTH];
                        playerOneState = PlayerOneState.AtNorth;
                        playerOneMoveStep = Vector2.Zero;
                    }
                    break;
                case PlayerOneState.CenterToSouth:
                    if (Vector2.Distance(playerOnePosition, insideNodePositions[SpaceStationSection.SOUTH]) < playerOneMoveSpeed) {
                        playerOnePosition = insideNodePositions[SpaceStationSection.SOUTH];
                        playerOneState = PlayerOneState.AtSouth;
                        playerOneMoveStep = Vector2.Zero;
                    }
                    break;
                case PlayerOneState.CenterToWest:
                    if (Vector2.Distance(playerOnePosition, insideNodePositions[SpaceStationSection.WEST]) < playerOneMoveSpeed) {
                        playerOnePosition = insideNodePositions[SpaceStationSection.WEST];
                        playerOneState = PlayerOneState.AtWest;
                        playerOneMoveStep = Vector2.Zero;
                    }
                    break;
                case PlayerOneState.CenterToEast:
                    if (Vector2.Distance(playerOnePosition, insideNodePositions[SpaceStationSection.EAST]) < playerOneMoveSpeed) {
                        playerOnePosition = insideNodePositions[SpaceStationSection.EAST];
                        playerOneState = PlayerOneState.AtEast;
                        playerOneMoveStep = Vector2.Zero;
                    }
                    break;
                case PlayerOneState.NorthToCenter:
                case PlayerOneState.SouthToCenter:
                case PlayerOneState.EastToCenter:
                case PlayerOneState.WestToCenter:
                    if (Vector2.Distance(playerOnePosition, insideNodePositions[SpaceStationSection.CENTER]) < playerOneMoveSpeed) {
                        playerOnePosition = insideNodePositions[SpaceStationSection.CENTER];
                        playerOneState = PlayerOneState.AtCenter;
                        playerOneMoveStep = Vector2.Zero;
                    }
                    break;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            int frame = (int)(gameTime.TotalGameTime.TotalMilliseconds / 125) % 4;

            switch (context) {
                case ScreenContext.TITLE_SCREEN_MENU:
                    spriteBatch.Draw(titleScreenMenu, Vector2.Zero, Color.White);
                    switch (currentMenuItem) {
                        case MenuItem.NEW_GAME:
                            spriteBatch.Draw(menuSelector[frame], firstMainMenuPosition, Color.White);
                            break;
                        case MenuItem.INSTRUCTIONS:
                            spriteBatch.Draw(menuSelector[frame], secondMainMenuPosition, Color.White);
                            break;
                        case MenuItem.CREDITS:
                            spriteBatch.Draw(menuSelector[frame], thirdMainMenuPosition, Color.White);
                            break;
                        case MenuItem.EXIT:
                            spriteBatch.Draw(menuSelector[frame], fourthMainMenuPosition, Color.White);
                            break;
                    }
                    break;
                case ScreenContext.INSTRUCTIONS:
                    spriteBatch.Draw(instructions, Vector2.Zero, Color.White);
                    break;
                case ScreenContext.CREDITS:
                    spriteBatch.Draw(credits, Vector2.Zero, Color.White);
                    break;
                case ScreenContext.CHARACTER_SELECTION:
                    spriteBatch.Draw(characterSelectionBackground, Vector2.Zero, Color.White);
                    if (currentPlayer == PlayerNumber.ONE) {
                        spriteBatch.Draw(menuSelector[frame], playerOneMenuPosition, Color.White);
                    } else {
                        spriteBatch.Draw(menuSelector[frame], playerTwoMenuPosition, Color.White);
                    }
                    break;
                case ScreenContext.KEY_CODE:
                    spriteBatch.Draw(keyCodeBackground, Vector2.Zero, Color.White);
                    if (currentPlayer == PlayerNumber.ONE) {
                        spriteBatch.DrawString(font, keyCode, new Vector2(685 - font.MeasureString(keyCode).X / 2, 210), Color.Yellow);
                    } else {
                        spriteBatch.DrawString(font, keyCode, new Vector2(685 - font.MeasureString(keyCode).X / 2, 210), Color.Yellow);
                        if (gameTime.TotalGameTime.TotalMilliseconds % 1000 > 500) {
                            spriteBatch.DrawString(font, "_", new Vector2(685 + font.MeasureString(keyCode).X / 2, 210), Color.Yellow);
                        }
                        if (Cheater.CheatsOn) {
                            spriteBatch.DrawString(font, "GadgetKeyDustChild", new Vector2(20, 20), Color.Yellow, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
                        }
                    }
                    break;
                case ScreenContext.READY_TO_START:
                    spriteBatch.Draw(readyStartBackground, Vector2.Zero, Color.White);
                    break;
                case ScreenContext.GAME_PLAY:

                    //Draw the background
                    if (currentPlayer == PlayerNumber.ONE) {
                        spriteBatch.Draw(playerOneBackground, Vector2.Zero, Color.White);

                        Vector2 centreValves = new Vector2(680, 370);
                        Vector2 eastValves = new Vector2(1020, 370);
                        Vector2 northValves = new Vector2(680, 60);
                        Vector2 southValves = new Vector2(680, 650);
                        Vector2 westValves = new Vector2(340, 370);
                        Vector2 offScreenValves = new Vector2(-1000, -1000);
                        Vector2 currentValves;

                        Vector2 centerDisplay = new Vector2(560, 310);
                        Vector2 eastDisplay = new Vector2(920, 310);
                        Vector2 westDisplay = new Vector2(240, 312);
                        Vector2 northDisplay = new Vector2(570, 30);
                        Vector2 southDisplay = new Vector2(570, 580);
                        Vector2 currentDisplayLoc;

                        EventSlot visibleSlot = EventSlot.Center;
                        bool somethingVisible = false;

                        //Draw the player
                        spriteBatch.Draw(playerOneSprite, new Vector2(playerOnePosition.X - playerOneSprite.Width / 2,
                            playerOnePosition.Y - playerOneSprite.Height / 2), Color.White);

                        switch (playerOneState) {
                            case PlayerOneState.AtCenter:
                                currentValves = centreValves;
                                currentDisplayLoc = centerDisplay;
                                spriteBatch.Draw(displayTexture, centerDisplay, Color.White);
                                visibleSlot = EventSlot.Center;
                                somethingVisible = true;
                                break;
                            case PlayerOneState.AtEast:
                                currentValves = eastValves;
                                currentDisplayLoc = eastDisplay;
                                spriteBatch.Draw(displayTexture, eastDisplay, Color.White);
                                visibleSlot = EventSlot.East;
                                somethingVisible = true;
                                break;
                            case PlayerOneState.AtNorth:
                                currentValves = northValves;
                                currentDisplayLoc = northDisplay;
                                spriteBatch.Draw(displayTexture, northDisplay, Color.White);
                                visibleSlot = EventSlot.North;
                                somethingVisible = true;
                                break;
                            case PlayerOneState.AtSouth:
                                currentValves = southValves;
                                currentDisplayLoc = southDisplay;
                                spriteBatch.Draw(displayTexture, southDisplay, Color.White);
                                visibleSlot = EventSlot.South;
                                somethingVisible = true;
                                break;
                            case PlayerOneState.AtWest:
                                currentValves = westValves;
                                currentDisplayLoc = westDisplay;
                                spriteBatch.Draw(displayTexture, westDisplay, Color.White);
                                visibleSlot = EventSlot.West;
                                somethingVisible = true;
                                break;
                            default:
                                currentValves = offScreenValves;
                                currentDisplayLoc = offScreenValves;
                                somethingVisible = false;
                                break;
                        }
                        spriteBatch.Draw(valvePanel, currentValves, Color.White);

                        float rotation = (float)(gameTime.TotalGameTime.TotalMilliseconds % 500 * 2 * Math.PI / 500);
                        switch (currentlyClosingValve) {
                            case GasValve.NONE:
                                break;
                            case GasValve.BLUE:
                                spriteBatch.DrawString(font, "O", new Vector2(currentValves.X + 19, currentValves.Y + 34), Color.White, rotation, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);
                                break;
                            case GasValve.YELLOW:
                                spriteBatch.DrawString(font, "O", new Vector2(currentValves.X + 34, currentValves.Y + 19), Color.White, rotation, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);
                                break;
                            case GasValve.GREEN:
                                spriteBatch.DrawString(font, "O", new Vector2(currentValves.X + 36, currentValves.Y + 52), Color.White, rotation, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);
                                break;
                            case GasValve.RED:
                                spriteBatch.DrawString(font, "O", new Vector2(currentValves.X + 52, currentValves.Y + 34), Color.White, rotation, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);
                                break;
                        }

                        foreach (DisasterEvent theEvent in disasterEvents) {
                            if (theEvent is RepairDisaster) {
                                RepairDisaster repairDis = theEvent as RepairDisaster;
                                if (somethingVisible && repairDis.Slot == visibleSlot) {
                                    switch (repairDis.StationPart) {
                                        case StationPart.Hatch:
                                            spriteBatch.Draw(hatchTexture, currentDisplayLoc + new Vector2(8, 8), Color.Red);
                                            break;
                                        case StationPart.O2Tank:
                                            spriteBatch.Draw(tankTexture, currentDisplayLoc + new Vector2(8, 8), Color.Blue);
                                            break;
                                        case StationPart.Pipe:
                                            spriteBatch.Draw(pipeTexture, currentDisplayLoc + new Vector2(8, 8), Color.Green);
                                            break;
                                        case StationPart.SatelliteDish:
                                            spriteBatch.Draw(satelliteDishTexture, currentDisplayLoc + new Vector2(8, 8), Color.Yellow);
                                            break;
                                    }
                                }
                            }
                        }
                    } else {
                        spriteBatch.Draw(playerTwoBackground, Vector2.Zero, Color.White);

                        //Draw all the doodads on the outside of the ship

                        foreach (RepairPart part in repairableParts) {
                            spriteBatch.Draw(part.Texture, part.Position - new Vector2(part.Texture.Width / 2, part.Texture.Height / 2), part.TintColor);
                        }

                        //Draw the player
                        spriteBatch.Draw(playerTwoSprite, new Vector2(playerTwoPosition.X - playerTwoSprite.Width / 2,
                            playerTwoPosition.Y - playerTwoSprite.Height / 2), Color.White);

                        //Draw the repair icons if close to the items to repair
                        drawRepairIcon(spriteBatch);
                    }
                    TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, (int)elapsedRoundTime);
                    spriteBatch.DrawString(font, string.Format("Time: {0,2:00}:{1,2:00}", timeSpan.Minutes, timeSpan.Seconds), new Vector2(10, 10), Color.White);
                    foreach (DisasterEvent disaster in disasterEvents) {
                        if (disaster.VisibleToPlayer == currentPlayer) {
                            disaster.Draw(spriteBatch);
                        }
                        if (Cheater.CheatsOn) {
                            spriteBatch.DrawString(font, "Event [" + disaster + "] time left: "
                                + (int)((disaster.EndTime - elapsedRoundTime) / 1000),
                                new Vector2(20, 50 + (disasterEvents.IndexOf(disaster) * 14)), Color.Red, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
                        }
                    }
                    break;
                case ScreenContext.DEATH:
                    TimeSpan deathTimeSpan = new TimeSpan(0, 0, 0, 0, (int)deathTime);
                    Vector2 measureString = font.MeasureString("GAME OVER");
                    spriteBatch.DrawString(font, "GAME OVER", new Vector2(640 - measureString.X / 2, 360 - measureString.Y / 2), Color.Red);
                    spriteBatch.DrawString(font, string.Format("Time: {0,2:00}:{1,2:00}", deathTimeSpan.Minutes, deathTimeSpan.Seconds), new Vector2(10, 10), Color.White);
                    break;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void drawRepairIcon(SpriteBatch spriteBatch) {
            //Whole lot of collision to check here
            float repairRadius = 16.0f;
            Vector2 repairIconOffset = new Vector2(-32, -32);

            foreach (RepairPart part in repairableParts) {
                if (Vector2.Distance(playerTwoPosition, part.Position) < repairRadius) {
                    if (part.Active) {
                        float rotation = (float)((elapsedRoundTime - repairEndTime)) / 1000;
                        spriteBatch.Draw(circleTexture, part.Position + repairIconOffset, circleTexture.Bounds, part.TintColor,
                            rotation, new Vector2(circleTexture.Width / 2, circleTexture.Height / 2), 1.0f, SpriteEffects.None, 1.0f);
                    } else {
                        spriteBatch.Draw(circleTexture, part.Position + repairIconOffset, part.TintColor);
                    }
                }
            }
        }

        private bool isNewlyPressedUp() {
            return (currentGamepadState.IsButtonDown(Buttons.DPadUp) && !previousGamepadState.IsButtonDown(Buttons.DPadUp))
                || (currentKeyboardState.IsKeyDown(Keys.Up) && !previousKeyboardState.IsKeyDown(Keys.Up))
                || (currentKeyboardState.IsKeyDown(Keys.W) && !previousKeyboardState.IsKeyDown(Keys.W));
        }

        private bool isNewlyPressedDown() {
            return (currentGamepadState.IsButtonDown(Buttons.DPadDown) && !previousGamepadState.IsButtonDown(Buttons.DPadDown))
                || (currentKeyboardState.IsKeyDown(Keys.Down) && !previousKeyboardState.IsKeyDown(Keys.Down))
                || (currentKeyboardState.IsKeyDown(Keys.S) && !previousKeyboardState.IsKeyDown(Keys.S));
        }

        private bool isNewlyPressedStart() {
            return (currentGamepadState.IsButtonDown(Buttons.Start) && !previousGamepadState.IsButtonDown(Buttons.Start))
                || (currentKeyboardState.IsKeyDown(Keys.Enter) && !previousKeyboardState.IsKeyDown(Keys.Enter))
                || (currentKeyboardState.IsKeyDown(Keys.Space) && !previousKeyboardState.IsKeyDown(Keys.Space));
        }

        private bool isNewlyPressedBack() {
            return (currentGamepadState.IsButtonDown(Buttons.Back) && !previousGamepadState.IsButtonDown(Buttons.Back))
                || (currentKeyboardState.IsKeyDown(Keys.Back) && !previousKeyboardState.IsKeyDown(Keys.Back))
                || (currentKeyboardState.IsKeyDown(Keys.Escape) && !previousKeyboardState.IsKeyDown(Keys.Escape));
        }

    }

    public enum PlayerNumber {
        ONE,
        TWO
    };

    public enum SpaceStationSection {
        NORTH,
        SOUTH,
        EAST,
        WEST,
        CENTER
    };

    public enum PlayerOneState {
        AtCenter,
        AtNorth,
        AtSouth,
        AtEast,
        AtWest,
        CenterToNorth,
        CenterToSouth,
        CenterToEast,
        CenterToWest,
        NorthToCenter,
        SouthToCenter,
        EastToCenter,
        WestToCenter
    };

    public enum InputKey {
        None,
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight
    };

    public enum ScreenContext {
        TITLE_SCREEN_MENU,
        INSTRUCTIONS,
        CREDITS,
        CHARACTER_SELECTION,
        KEY_CODE,
        READY_TO_START,
        GAME_PLAY,
        DEATH
    }

    public enum MenuItem {
        NEW_GAME,
        INSTRUCTIONS,
        CREDITS,
        EXIT
    }

    public enum GasValve {
        NONE,
        BLUE,
        YELLOW,
        GREEN,
        RED
    }

    public class RepairPart {

        public RepairPart(bool active, StationPart part, EventSlot slot, Vector2 position, Texture2D texture) {
            Active = active;
            Part = part;
            Slot = slot;
            Position = position;
            Texture = texture;

            switch (part) {
                case StationPart.Hatch:
                    TintColor = Color.Red;
                    break;
                case StationPart.O2Tank:
                    TintColor = Color.Blue;
                    break;
                case StationPart.Pipe:
                    TintColor = Color.Green;
                    break;
                case StationPart.SatelliteDish:
                    TintColor = Color.Yellow;
                    break;
            }
        }

        public bool Active {
            get;
            set;
        }
        public StationPart Part {
            get;
            set;
        }
        public EventSlot Slot {
            get;
            set;
        }
        public Vector2 Position {
            get;
            set;
        }
        public Texture2D Texture {
            get;
            set;
        }

        public Color TintColor {
            get;
            private set;
        }
    }
}
