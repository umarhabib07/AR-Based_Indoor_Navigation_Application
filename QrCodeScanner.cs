using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public class QrCodeScanner : MonoBehaviour
{
    [SerializeField]
    private ARSession session;

    [SerializeField]
    public Button scanButton;


    [SerializeField]

    public SetNavigationTarget obj;

    [SerializeField]
    private ARSessionOrigin sessionOrigin;

    [SerializeField]
    private ARCameraManager cameraManager;

    [SerializeField]
    public GameObject scanmessagedisplay;

    [SerializeField]
    private List<Target> navigationTargetObjects = new List<Target>();


    public Texture2D cameraImageTexture;
    private IBarcodeReader reader = new BarcodeReader(); // create a barcode reader instance


    void Start()
    {
        scanButton.onClick.AddListener(ScanQRCode);
    }


    private void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }


    private void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }


    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {


    }

    private void ScanQRCode()
    {
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            StartCoroutine(obj.ShowMessagePopup("Scan successful"));
            obj.EnableObjects();
            scanButton.gameObject.SetActive(false);
            scanmessagedisplay.gameObject.SetActive(false);

            return;
        }

        var conversionParams = new XRCpuImage.ConversionParams
        {
            // Get the entire image.
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 2.
            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

            // Choose RGBA format.
            outputFormat = TextureFormat.RGBA32,

            // Flip across the vertical axis (mirror image).
            transformation = XRCpuImage.Transformation.MirrorY
        };

        // See how many bytes you need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        // Allocate a buffer to store the image.
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data
        image.Convert(conversionParams, buffer);

        // The image was converted to RGBA32 format and written into the provided buffer
        // so you can dispose of the XRCpuImage. You must do this or it will leak resources.
        image.Dispose();

        // At this point, you can process the image, pass it to a computer vision algorithm, etc.
        // In this example, you apply it to a texture to visualize it.

        // You've got the data; let's put it into a texture so you can visualize it.
        cameraImageTexture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false);

        cameraImageTexture.LoadRawTextureData(buffer);
        cameraImageTexture.Apply();

        // Done with your temporary data, so you can dispose it.
        buffer.Dispose();

        // Detect and decode the barcode inside the bitmap
        var result = reader.Decode(cameraImageTexture.GetPixels32(), cameraImageTexture.width, cameraImageTexture.height);

        // Do something with the result
        if (result != null)
        {
            SetQrCodeRecenterTarget(result.Text);
        }
        else
        {
            StartCoroutine(obj.ShowMessagePopup("Scan not successful"));
        }

    }

    private void SetQrCodeRecenterTarget(string targetText)
    {
        Target currentTarget = navigationTargetObjects.Find(x => x.Name.Equals(targetText));
        if (currentTarget != null)
        {

            StartCoroutine(obj.ShowMessagePopup("Scan successful"));
            obj.EnableObjects();
            scanButton.gameObject.SetActive(false);
            scanmessagedisplay.gameObject.SetActive(false);

            // Reset position and rotation of ARSession
            session.Reset();

            // Add offset for recentering
            sessionOrigin.transform.position = currentTarget.PositionObject.transform.position;
            sessionOrigin.transform.rotation = currentTarget.PositionObject.transform.rotation;
        }

    }


}
