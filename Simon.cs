using Godot;
using System;

public class Simon : Node {

    private Node2D[] buttons;
    private Node2D[] fakeButtons;


    public override void _Ready() {
        GD.Print("hej");

        buttons = new Node2D[4];
        fakeButtons = new Node2D[4];

        string[] buttonLabels = new []{
            "BtnA", "BtnB", "BtnC", "BtnD"
        };


        //var viewportSize = GetViewport().Size;
        

        for (int i=0; i<4; i++) {
            var n = GetNode(buttonLabels[i]);
            n.Connect("pressed", this, buttonLabels[i] + "Dn");
            n.Connect("released", this, buttonLabels[i] + "Up");

            buttons[i] = (Node2D)n;
            fakeButtons[i] = GetNode<Node2D>(buttonLabels[i] + "Fake");

            fakeButtons[i].Position = buttons[i].Position;
            fakeButtons[i].Visible = false;
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
        GD.Print("Button down: " + i);
    }
    private void OnButtonUp(int i) {
        GD.Print("Button up: " + i);
    }




//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
