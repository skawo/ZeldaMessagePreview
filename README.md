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
