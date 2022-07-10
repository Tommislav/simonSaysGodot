using Godot;
using System;
using System.Collections.Generic;

public class Simon : Node {

    // --- temporary sound list ---
    [Export]
    public AudioStream sound1;
    [Export]
    public AudioStream sound2;
    [Export]
    public AudioStream sound3;
    [Export]
    public AudioStream sound4;

    [Export]
    public AudioStream newRoundSound;
    [Export]
    public AudioStream gameOverSound;
    // --- temporary sound list ---
    
    private Node2D[] clickables;
    private Node2D[] buttonsOn;
    private Node2D[] buttonsOff;
    private AudioStream[] soundList;
    private AudioStreamPlayer2D buttonClickAudioPlayer;
    private AudioStreamPlayer2D gameStateAudioPlayer;



    // Here are the game specific variables we need

    private enum Cmd {
        StartNewGame,
        StartNextRound,
        ShowNextButtonInSequence,
        ClearAllButtons,
        AwaitPlayerInput,
        GameOver
    }
    private Cmd nextCommand; // What command to execute once countdown is <= zero
    private float countdown; // Pause execution of next command
    private List<int> sequence = new List<int>(); // list of order, 0-3
    private List<int> playerInput = new List<int>();
    private int sequenceIndex; // where in the sequence are we?
    private System.Random rng;




    public override void _Ready() {
        rng = new System.Random();

        buttonClickAudioPlayer = GetNode<AudioStreamPlayer2D>("ButtonClickAudioPlayer");
        gameStateAudioPlayer = GetNode<AudioStreamPlayer2D>("GameStateAudioPlayer");

        soundList = new[] { sound1, sound2, sound3, sound4 };
        
        clickables = new Node2D[4];
        buttonsOn = new Node2D[4];
        buttonsOff = new Node2D[4];

        // setup button callbacks and size/positions
        string[] buttonLabels = new []{ "BtnA", "BtnB", "BtnC", "BtnD" };

        var viewportSize = GetViewport().Size;
        float w = Mathf.Min(viewportSize.x, viewportSize.y);
        float margin = w / 20f;
        w -= margin * 3;
        float buttonExpectedW = w / 2f;
        float halfW = viewportSize.x / 2f;
        float halfH = viewportSize.y / 2f;
        float halfM = margin / 2f;

        // assume all buttons/assets are same widht/height (at least for now)
        var size = GetNode<Sprite>(buttonLabels[0] + "On").Texture.GetSize();
        Vector2 scale = new Vector2(buttonExpectedW / size.x, buttonExpectedW / size.y);

        float x1 = halfW - buttonExpectedW - halfM;
        float x2 = halfW + halfM;
        float y1 = halfH - buttonExpectedW - halfM;
        float y2 = halfH + halfM;

        Vector2[] positions = new[] {
            new Vector2(x1, y1),
            new Vector2(x2, y1),
            new Vector2(x1, y2),
            new Vector2(x2, y2)
        };
        
        for (int i=0; i<4; i++) {
            buttonsOn[i] = GetNode<Node2D>(buttonLabels[i] + "On");
            
            
            var n = GetNode<Node2D>(buttonLabels[i]);
            n.Connect("pressed", this, buttonLabels[i] + "Dn");
            n.Connect("released", this, buttonLabels[i] + "Up");
            clickables[i] = n;
            buttonsOff[i] = GetNode<Node2D>(buttonLabels[i] + "Off");

            buttonsOn[i].Scale = scale;
            buttonsOn[i].Position = positions[i];
            buttonsOff[i].Scale = scale;
            buttonsOff[i].Position = positions[i];
            clickables[i].Scale = scale;
            clickables[i].Position = positions[i];
            buttonsOn[i].Visible = false;
        }

        nextCommand = Cmd.StartNewGame;
    }

    private void BtnADn() { SetButtonState(0, true); }
    private void BtnBDn() { SetButtonState(1, true); }
    private void BtnCDn() { SetButtonState(2, true); }
    private void BtnDDn() { SetButtonState(3, true); }

    private void BtnAUp() { SetButtonState(0, false); }
    private void BtnBUp() { SetButtonState(1, false); }
    private void BtnCUp() { SetButtonState(2, false); }
    private void BtnDUp() { SetButtonState(3, false); }


    private void SetButtonState(int b, bool on, bool muteSounds=false) {
        for(int i = 0; i < 4; i++) {
            buttonsOff[i].Visible = b != i || !on;
            buttonsOn[i].Visible = b == i && on;
        }

        if (on && !muteSounds) {
            buttonClickAudioPlayer.Stream = soundList[b];
            buttonClickAudioPlayer.Play();
        }
        if (!on && nextCommand == Cmd.AwaitPlayerInput) {
            playerInput.Add(b);
        }
    }

    private void SetButtonsClickable(bool on) {
        for(int i = 0; i < 4; i++) { clickables[i].Visible = on; }
    }


    public override void _Process(float delta) {
        countdown -= delta;
        if (countdown > 0) { return; }

        switch(nextCommand) {
            case Cmd.StartNewGame: {
                sequence.Clear();
                SetButtonsClickable(false);
                nextCommand = Cmd.StartNextRound;
                break;
            }
            case Cmd.StartNextRound: {
                gameStateAudioPlayer.Stream = newRoundSound;
                gameStateAudioPlayer.Play();

                playerInput.Clear();
                sequence.Add(rng.Next(0,4));
                sequenceIndex = 0;
                SetButtonsClickable(false);
                countdown = 1f;
                nextCommand = Cmd.ShowNextButtonInSequence;
                break;
            }
            case Cmd.ShowNextButtonInSequence: {
                if (sequenceIndex < sequence.Count) {
                    SetButtonState(sequence[sequenceIndex], true);
                    nextCommand = Cmd.ClearAllButtons;
                    countdown = 0.7f;
                    sequenceIndex++;
                }
                else {
                    SetButtonsClickable(true);
                    sequenceIndex = 0;
                    nextCommand = Cmd.AwaitPlayerInput;
                }
                break;
            }
            case Cmd.ClearAllButtons: {
                SetButtonState(0, false);
                countdown = 0.3f;
                nextCommand = Cmd.ShowNextButtonInSequence;
                break;
            }

            case Cmd.AwaitPlayerInput: {
                bool invalid = false;
                int num = Mathf.Min(sequence.Count, playerInput.Count);
                for (int i=0; i<num; i++) {
                    if (sequence[i] != playerInput[i]) {
                        invalid = true;
                    }
                }
                if (invalid) {
                    SetButtonsClickable(false);
                    nextCommand = Cmd.GameOver;
                    sequenceIndex = 0;

                    gameStateAudioPlayer.Stream = gameOverSound;
                    gameStateAudioPlayer.Play();
                }
                else if (sequence.Count == playerInput.Count) {
                    nextCommand = Cmd.StartNextRound;
                    SetButtonsClickable(false);
                    countdown = 0.5f;
                }

                break;
            }
            case Cmd.GameOver: {

                int btnIndex = sequenceIndex % 4;
                if (btnIndex == 2) btnIndex = 3;
                else if (btnIndex == 3) btnIndex = 2;
                SetButtonState(btnIndex, true, true);
                countdown = 0.08f;
                sequenceIndex++;

                if (sequenceIndex > 30) {
                    SetButtonState(0, false);
                    nextCommand = Cmd.StartNewGame;
                }

                break;
            }

        }




    }
}
