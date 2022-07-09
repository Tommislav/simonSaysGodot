using Godot;
using System;

public class Simon : Node {

    [Export]
    public AudioStream sound1;
    [Export]
    public AudioStream sound2;
    [Export]
    public AudioStream sound3;
    [Export]
    public AudioStream sound4;
    
    private Node2D[] clickables;
    private Node2D[] buttonsOn;
    private Node2D[] buttonsOff;
    private AudioStream[] sounds;
    private AudioStreamPlayer2D audioPlayer;


    public override void _Ready() {
        GD.Print("hej");

        audioPlayer = GetNode<AudioStreamPlayer2D>("AudioPlayer");

        sounds = new[] { sound1, sound2, sound3, sound4 };
        
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
        audioPlayer.Stream = sounds[i];
        audioPlayer.Play();
        SetButtonState(i, true);
    }
    private void OnButtonUp(int i) {
        SetButtonState(i, false);
    }

    private void SetButtonState(int b, bool on) {
        for(int i = 0; i < 4; i++) {
            buttonsOff[i].Visible = b != i || !on;
            buttonsOn[i].Visible = b == i && on;
        }
    }




//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
