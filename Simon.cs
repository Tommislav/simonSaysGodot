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
    private AudioStreamPlayer2D audioPlayer;
    private AudioStreamPlayer2D audioEffectsPlayer;



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

        audioPlayer = GetNode<AudioStreamPlayer2D>("AudioPlayer");
        audioEffectsPlayer = GetNode<AudioStreamPlayer2D>("Effects");

        soundList = new[] { sound1, sound2, sound3, sound4 };
        
        clickables = new Node2D[4];
        buttonsOn = new Node2D[4];
        buttonsOff = new Node2D[4];

        string[] buttonLabels = new []{
            "BtnA", "BtnB", "BtnC", "BtnD"
        };


        var viewportSize = GetViewport().Size;
        float w = Mathf.Min(viewportSize.x, viewportSize.y);
        float margin = w / 20f;
        w -= margin * 3;
        float buttonExpectedW = w / 2f;
        float halfW = viewportSize.x / 2f;
        float halfH = viewportSize.y / 2f;

        // assume all buttons are same size (for now)
        var size = GetNode<Sprite>(buttonLabels[0] + "On").Texture.GetSize();
        Vector2 scale = new Vector2(buttonExpectedW / size.x, buttonExpectedW / size.y);

        Vector2[] positions = new[] {
            new Vector2(margin, margin),
            new Vector2(buttonExpectedW + margin * 2, margin),
            new Vector2(margin, buttonExpectedW + margin * 2),
            new Vector2(buttonExpectedW + margin * 2, buttonExpectedW + margin * 2)
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

    private void BtnADn() { OnButtonDn(0); }
    private void BtnBDn() { OnButtonDn(1); }
    private void BtnCDn() { OnButtonDn(2); }
    private void BtnDDn() { OnButtonDn(3); }

    private void BtnAUp() { OnButtonUp(0); }
    private void BtnBUp() { OnButtonUp(1); }
    private void BtnCUp() { OnButtonUp(2); }
    private void BtnDUp() { OnButtonUp(3); }


    private void OnButtonDn(int i) {
        SetButtonState(i, true);
    }
    private void OnButtonUp(int i) {
        SetButtonState(i, false);
    }

    private void SetButtonState(int b, bool on, bool muteSounds=false) {
        for(int i = 0; i < 4; i++) {
            buttonsOff[i].Visible = b != i || !on;
            buttonsOn[i].Visible = b == i && on;
        }

        if (on && !muteSounds) {
            audioPlayer.Stream = soundList[b];
            audioPlayer.Play();
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
                audioEffectsPlayer.Stream = newRoundSound;
                audioEffectsPlayer.Play();

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

                    audioEffectsPlayer.Stream = gameOverSound;
                    audioEffectsPlayer.Play();
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
