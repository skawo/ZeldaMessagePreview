A library for rendering previews of Zelda OoT textboxes.
Most of OoT's messages should be rendered 1:1 to how they appear in the game,
with the exception of messages using a <Background> tag (0x15).
  
Supports custom fonts as exported from <a href="https://github.com/z64me/z64font">z64font</a>. Simply put both files exported from the program in the same directory as the dll - named ''font.font_static'' and ''font.width_table'' and they will be automatically loaded and used for the preview.


<b>Usage</b>
```csharp
// Get a byte array with the message data in OoT's format.
byte[] msg = System.Text.Encoding.Unicode.GetBytes($"Hello world!");

// Create the preview class.
MessagePreview preview = new MessagePreview(ZeldaMessagePreview.Data.BoxType.Black, msg);

// Get and save the bitmap.
Bitmap bmp = preview.GetPreview();
bmp.Save("out.png");
```
Result:<br><br>
![out](https://user-images.githubusercontent.com/43761362/130130470-b00bb939-e525-4d93-9365-5175393c166d.png)



