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
Result:
![image](https://user-images.githubusercontent.com/43761362/130130443-7e9fa915-7e7e-408a-85ef-69434bafb035.png)


