A library for rendering previews of Zelda OoT textboxes.

<b>Usage</b>
```csharp
// Get a byte array with the message data in OoT's format.
byte[] msg = System.Text.Encoding.Unicode.GetBytes($"Hello world!");

// Create the preview class.
MessagePreview preview = new MessagePreview(Data.BoxType.Black, msg);

// Get and save the bitmap.
Bitmap bmp = msgP1.GetPreview();
bmp.Save("out.png");
```
Result:<br><br>
![out](https://user-images.githubusercontent.com/43761362/130130470-b00bb939-e525-4d93-9365-5175393c166d.png)



