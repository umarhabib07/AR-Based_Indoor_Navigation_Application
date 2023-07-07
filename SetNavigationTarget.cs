using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;


public class SetNavigationTarget : MonoBehaviour
{

    //  [SerializeField]
    //   private ARSession arSession;
    // [SerializeField] 
    // private ARSessionOrigin arSessionOrigin;

    [SerializeField]
    private TextMeshProUGUI distanceText;

    private bool turnMessageDisplayed = false;
    private float lastTurnMessageTime = 0f;

    [SerializeField]

    public QrCodeScanner obj;

    [SerializeField]
    private TMP_Dropdown navigationTargetDropDown;

    [SerializeField]
    private Camera topDownCamera;

    [SerializeField]
    private List<Target> navigationTargetObjects = new List<Target>();

    [SerializeField]
    private Button clearButton;

    [SerializeField]
    private Button BackButton;

    [SerializeField]
    private Button homeButton;

    [SerializeField]
    private Button exitButton;

    [SerializeField]
    private Image scanimage;

    [SerializeField]
    private Image endmessage;

    [SerializeField]
    public RawImage rawImage;

    [SerializeField]
    private GameObject messagePopup;

    // public bool enable=true;


    private Vector3 targetPosition = Vector3.zero; //current target position

    private NavMeshPath path;
    private LineRenderer line;
    private bool lineToggle = false;
    private bool navigating = false;

    private void Start()
    {

        path = new NavMeshPath();
        line = transform.GetComponent<LineRenderer>();


        clearButton.onClick.AddListener(ClearNavigation);

        BackButton.onClick.AddListener(BackFuntion);

        homeButton.onClick.AddListener(homeFuntion);

        exitButton.onClick.AddListener(exitFuntion);

        scanimage.gameObject.SetActive(true);
        clearButton.gameObject.SetActive(false);
        BackButton.gameObject.SetActive(false);
        navigationTargetDropDown.gameObject.SetActive(false);
        rawImage.gameObject.SetActive(false);
        homeButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);
        endmessage.gameObject.SetActive(false);


        // Disable all target objects
        foreach (var target in navigationTargetObjects)
        {
            target.PositionObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (lineToggle && targetPosition != Vector3.zero)
        {
            // Calculate the path using NavMesh
            // NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);

            // Set the position count of the line renderer to the length of the path  
            line.positionCount = path.corners.Length;

            // Set the start position of the line to the current position of the object
            Vector3 startPosition = transform.position;

            // Loop through each corner of the path and set the position of the line renderer
            for (int i = 0; i < path.corners.Length; i++)
            {
                Vector3 endPosition = path.corners[i];

                // Raycast from the start position to the end position to check for obstacles
                RaycastHit hit;
                if (Physics.Raycast(startPosition, (endPosition - startPosition).normalized, out hit, (endPosition - startPosition).magnitude))
                {
                    // If an obstacle was hit, adjust the end position to the point of collision
                    endPosition = hit.point;
                }

                // Set the position of the line renderer
                line.SetPosition(i, endPosition);

                // Set the start position of the next line segment to the end position of the current segment
                startPosition = endPosition;
            }

            // Enable or disable the line renderer based on the value of the lineToggle variable
            line.enabled = lineToggle;

            // NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);
            // line.positionCount = path.corners.Length;
            // line.SetPositions(path.corners);
            // line.enabled = lineToggle;

            // const float yOffset = -0.1f;
            // for (int i = 0; i < path.corners.Length; i++)
            // {
            //     var point = path.corners[i];
            //     point.y += yOffset;
            //     line.SetPosition(i, point);
            // }


            // Calculate the distance between the current position and the target position
            float distance = Vector3.Distance(transform.position, targetPosition);
            distanceText.text = "Distance: " + distance.ToString("F2") + "m";

            // Determine the direction to the waypoint
            Vector3 directionToWaypoint = (targetPosition - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToWaypoint);

            string turnDirection = "";

            // Check if near turn and display turn message if necessary
            if (distance <= 5f && !turnMessageDisplayed)
            {
                if (dotProduct > 0)
                {
                    turnDirection = "Turn right";
                    distanceText.text = "Distance: " + distance.ToString("F2") + "m\n" + turnDirection;
                }
                else
                {
                    turnDirection = "Turn left";
                    distanceText.text = "Distance: " + distance.ToString("F2") + "m\n" + turnDirection;
                }
                turnMessageDisplayed = true;
                lastTurnMessageTime = Time.time;
            }

            // Remove turn message after 4 seconds
            if (turnMessageDisplayed && Time.time - lastTurnMessageTime >= 5f)
            {
                distanceText.text = "Distance: " + distance.ToString("F2") + "m";
                turnMessageDisplayed = false;
            }

            // Show pop-up message when user reaches the destination
            if (distance <= 1.0f && navigating)
            {
                navigating = false;
                lineToggle = false;
                line.enabled = false;
                endmessage.gameObject.SetActive(true);
                homeButton.gameObject.SetActive(true);
                exitButton.gameObject.SetActive(true);
                navigationTargetDropDown.gameObject.SetActive(false);
                clearButton.gameObject.SetActive(false);
            }

        }
    }



    public void SetCurrentNavigationTarget(int selectedValue)
    {
        if (navigating)
        {
            StartCoroutine(ShowMessagePopup("Please clear the navigation first..!"));
            navigationTargetDropDown.value = navigationTargetDropDown.options.FindIndex(option => option.text == GetCurrentNavigationTarget().Name);
            return;
        }

        targetPosition = Vector3.zero;
        string selectedText = navigationTargetDropDown.options[selectedValue].text;
        Target currentTarget = navigationTargetObjects.Find(x => x.Name.Equals(selectedText));
        if (currentTarget != null)
        {
            targetPosition = currentTarget.PositionObject.transform.position;
            lineToggle = true;
            navigating = true;

            // Update the dropdown selection
            navigationTargetDropDown.value = selectedValue;

            // BackButton.gameObject.SetActive(false);


            // Disable all target objects
            foreach (var target in navigationTargetObjects)
            {
                target.PositionObject.SetActive(false);
            }

            // Enable the selected target object
            currentTarget.PositionObject.SetActive(true);

            // Start a rotation animation for the selected target object
            StartCoroutine(RotateObject(currentTarget.PositionObject, true));


            clearButton.gameObject.SetActive(true);


        }
    }



    public void ClearNavigation()
    {

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        // obj.cameraImageTexture=null;

        // clearButton.gameObject.SetActive(false);
        // navigationTargetDropDown.gameObject.SetActive(false);
        // rawImage.gameObject.SetActive(false);
        // obj.scanButton.gameObject.SetActive(true);
        // obj.scanmessagedisplay.gameObject.SetActive(true);


        // lineToggle = false;
        // line.enabled = false;
        // navigating = false;

        // // Clear the current target position
        // targetPosition = Vector3.zero;
        // foreach (var target in navigationTargetObjects)
        // {
        //     target.PositionObject.SetActive(false);
        // }

        // // Reset the dropdown selection
        // navigationTargetDropDown.value = -1;

    }

    public void BackFuntion()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void homeFuntion()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void exitFuntion()
    {
        // Quit the application
        Application.Quit();
    }

    private Target GetCurrentNavigationTarget()
    {
        if (targetPosition == Vector3.zero)
        {
            return null;
        }

        foreach (var target in navigationTargetObjects)
        {
            if (target.PositionObject.transform.position == targetPosition)
            {
                return target;
            }
        }

        return null;
    }

    public IEnumerator ShowMessagePopup(string message)
    {
        messagePopup.SetActive(true);
        messagePopup.GetComponentInChildren<TextMeshProUGUI>().text = message;
        yield return new WaitForSeconds(3f);
        messagePopup.SetActive(false);
    }

    public void EnableObjects()
    {
        // scanmessage.gameObject.SetActive(false);
        // clearButton.gameObject.SetActive(true);
        scanimage.gameObject.SetActive(false);
        BackButton.gameObject.SetActive(true);
        navigationTargetDropDown.gameObject.SetActive(true);
        rawImage.gameObject.SetActive(true);
    }


    private IEnumerator RotateObject(GameObject obj, bool rotateRight)
    {
        Quaternion startRotation = obj.transform.rotation;
        Quaternion targetRotation = startRotation;
        float angle = rotateRight ? 30f : -30f;

        while (true)
        {
            targetRotation *= Quaternion.Euler(new Vector3(0f, angle, 0f));
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 1.2f;
                obj.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                yield return null;
            }
            startRotation = obj.transform.rotation;
            yield return null;
        }
    }


}
