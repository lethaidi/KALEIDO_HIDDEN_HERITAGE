using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class MascotController : MonoBehaviour
{
    [SerializeField] private ProvinceData currentProvince;
    private ProvinceData targetProvince;

    private enum MoveMode
    {
        Ground,
        Air
    }

    private MoveMode currentMode = MoveMode.Ground;

    [Header("Ground Movement")]
    [SerializeField, Min(0.01f)] private float groundDuration = 2f;
    private float groundProgress = 0f;
    private Vector3 groundStartPosition;

    private bool isMoving = false;

    [SerializeField] private float flightThreshold = 5f;

    [Header("Bezier")]
    [SerializeField] private float curveOffset = -2f;
    [SerializeField] private float arcHeight = 3f;
    [SerializeField] private float flightDuration = 2f;
    private Vector3 p1;
    private float flightProgress = 0;

    private Vector3 originalScale;
    [SerializeField] private float maxScaleBoost = 0.5f;

    bool isSwitch = false;

    [Header("Mascot")]
    [SerializeField] private GameObject mascotVisual;
    [SerializeField] private GameObject airplaneVisual;
    [SerializeField] private GameObject busVisual;
    private Vector3 busDirection;

    [SerializeField] private ProvinceData lastProvinceVN;
    [SerializeField] private ProvinceData lastProvinceHQ;
    public ProvinceView[] provinceViews;
    private bool isInVietNam = true;

    [Header("Camera")]
    [SerializeField] private GameObject mainCameraVN;
    [SerializeField] private GameObject mainCameraHQ;
    [SerializeField] private GameObject mainCamera;

    [Header("UI")]
    [SerializeField] private RectTransform cloudLeft;
    [SerializeField] private RectTransform cloudRight;
    [SerializeField] private float cloudOpenX;
    [SerializeField] private float cloudClosedX;
    [SerializeField] private float cloudSpeed = 2f;
    [SerializeField] private float cloudCloseDuration = 2f;
    private float cloudTimer = 0f;

    public bool isCloudClosing;

    [Header("Wave Manager")]
    private Wave_manager wave_Manager;
    private int index;

    private void Start()
    {
        if (wave_Manager == null) wave_Manager = Object.FindAnyObjectByType<Wave_manager>();

        cloudTimer = cloudCloseDuration;

        mascotVisual.SetActive(true);
        airplaneVisual.SetActive(false);

        if (currentProvince == null || currentProvince.standPoint == null)
        {
            Debug.LogError("StandPoint is NULL");
            return;
        }
        transform.position = currentProvince.standPoint.position;

        originalScale = transform.localScale;

        if (isInVietNam)
        {
            mainCameraVN.SetActive(true);
            mainCameraHQ.SetActive(false);
        }
        else
        {
            mainCameraVN.SetActive(false);
            mainCameraHQ.SetActive(true);
        }

    }

    public void MoveToProvince(ProvinceData destination, int ind)
    {
        if (isMoving) return;

        if (destination == null || destination.standPoint == null)
        {
            Debug.LogError("Destination is NULL");
            return;
        }

        /*
        if (destination == currentProvince)
        {
            return;
        }*/

        targetProvince = destination;
        float dis = Vector3.Distance(currentProvince.standPoint.position, destination.standPoint.position);
        if (dis <= flightThreshold && mascotVisual.activeSelf)
        {
            currentMode = MoveMode.Ground;

            mascotVisual.SetActive(false);
            airplaneVisual.SetActive(false);

            groundStartPosition = transform.position;
            groundProgress = 0f;

            busVisual.transform.position = groundStartPosition;
            busVisual.SetActive(true);

            busDirection = currentProvince.standPoint.position - destination.standPoint.position;

            if (busDirection.sqrMagnitude > 0.0001f)
            {
                busVisual.transform.rotation =
                    Quaternion.LookRotation(busDirection);
            }
        }
        else
        {
            currentMode = MoveMode.Air;

            mascotVisual.SetActive(false);
            airplaneVisual.SetActive(true);

            p1 = Vector3.Lerp(currentProvince.standPoint.position, targetProvince.standPoint.position, 0.5f);
            p1 += new Vector3(curveOffset, arcHeight, 0f);

            flightProgress = 0f;
        }

        index = ind;
        isMoving = true;
    }

    public void SwitchCountry()
    {
        if (isMoving || isCloudClosing) return;

        if (isInVietNam)
        {
            lastProvinceVN = currentProvince;
            isInVietNam = false;
            MoveToProvince(lastProvinceHQ, -1);
        }
        else
        {
            lastProvinceHQ = currentProvince;
            isInVietNam = true;
            MoveToProvince(lastProvinceVN, -1);
        }

        isCloudClosing = true;
        isSwitch = true;
        cloudTimer = cloudCloseDuration;
    }

    private void Update()
    {
        if (isCloudClosing)
        {
            cloudLeft.anchoredPosition = Vector2.MoveTowards(cloudLeft.anchoredPosition, new Vector2(-cloudClosedX, cloudLeft.anchoredPosition.y), cloudSpeed * Time.deltaTime);
            cloudRight.anchoredPosition = Vector2.MoveTowards(cloudRight.anchoredPosition, new Vector2(cloudClosedX, cloudRight.anchoredPosition.y), cloudSpeed * Time.deltaTime);

            cloudTimer -= Time.deltaTime;

            if (cloudTimer <= 0)
            {
                isCloudClosing = false;
                cloudTimer = cloudCloseDuration;
            }
        }
        else
        {
            cloudLeft.anchoredPosition = Vector2.MoveTowards(cloudLeft.anchoredPosition, new Vector2(-cloudOpenX, cloudLeft.anchoredPosition.y), cloudSpeed * Time.deltaTime);
            cloudRight.anchoredPosition = Vector2.MoveTowards(cloudRight.anchoredPosition, new Vector2(cloudOpenX, cloudRight.anchoredPosition.y), cloudSpeed * Time.deltaTime);

        }

        if (!isMoving) return;

        Vector3 targetPosition = targetProvince.standPoint.position;

        if (currentMode == MoveMode.Ground)
        {
            groundProgress += Time.deltaTime / groundDuration;
            groundProgress = Mathf.Clamp01(groundProgress);

            // Smooth movement
            float t = Mathf.SmoothStep(0f, 1f, groundProgress);

            busVisual.transform.position = Vector3.Lerp(
                groundStartPosition,
                targetPosition,
                t
            );

            if (groundProgress >= 1f)
            {
                transform.position = targetPosition;
                currentProvince = targetProvince;

                busVisual.transform.position = targetPosition;
                busVisual.SetActive(false);

                mascotVisual.SetActive(true);

                isMoving = false;

                PlayWaveIndex(index);
            }
        }
        else if (currentMode == MoveMode.Air)
        {
            flightProgress += Time.deltaTime / flightDuration;
            flightProgress = Mathf.Clamp01(flightProgress);

            Vector3 pointA = Vector3.Lerp(currentProvince.standPoint.position, p1, flightProgress);
            Vector3 pointB = Vector3.Lerp(p1, targetProvince.standPoint.position, flightProgress);
            transform.position = Vector3.Lerp(pointA, pointB, flightProgress);

            Vector3 flightDirection = pointB - pointA;
            if (flightDirection != Vector3.zero)
            {
                airplaneVisual.transform.rotation = Quaternion.LookRotation(flightDirection);
            }

            float scaleCurve = Mathf.Sin(flightProgress * Mathf.PI);
            transform.localScale = originalScale + (Vector3.one * scaleCurve * maxScaleBoost);

            if (flightProgress >= 1f)
            {
                transform.position = targetPosition;
                currentProvince = targetProvince;
                transform.localScale = originalScale;


                airplaneVisual.SetActive(false);
                mascotVisual.SetActive(true);

                isMoving = false;

                if (!isSwitch)
                    PlayWaveIndex(index);

                isSwitch = false;
            }

            if (!isInVietNam && cloudTimer <= 0.5f)
            {
                mainCameraVN.SetActive(false);
                mainCameraHQ.SetActive(true);
            }
            else if (isInVietNam && cloudTimer <= 0.5f)
            {
                mainCameraVN.SetActive(true);
                mainCameraHQ.SetActive(false);
            }
        }
    }

    public void Replay()
    {
        if (wave_Manager != null)
            PlayWaveIndex(wave_Manager.wave_index);
    }

    public void PlayNext()
    {
        if (wave_Manager != null)
            PlayWaveIndex(wave_Manager.wave_index + 1);
    }
    
    private void PlayWaveIndex(int index)
    {
        DisableAllProvinceView();

        isCloudClosing = true;
        mascotVisual.SetActive(false);

        StartCoroutine(DelayStartScene(index));
    }

    private IEnumerator DelayStartScene(int index)
    {
        yield return new WaitForSeconds(1f);

        mainCamera.SetActive(true);
        mainCameraVN.SetActive(false);
        mainCameraHQ.SetActive(false);

        if (wave_Manager != null) wave_Manager.StartWave(index);
    }

    public void ReturnMap()
    {
        isCloudClosing = true;
        StartCoroutine(DelayReturnMap());
    }

    private IEnumerator DelayReturnMap()
    {
        yield return new WaitForSeconds(1f);

        mascotVisual.SetActive(true);
        mainCamera.SetActive(false);

        if (!isInVietNam)
        {
            mainCameraVN.SetActive(false);
            mainCameraHQ.SetActive(true);
        }
        else if (isInVietNam)
        {
            mainCameraVN.SetActive(true);
            mainCameraHQ.SetActive(false);
        }

        if (wave_Manager != null)
        {
            wave_Manager.wave_screen.SetActive(false);
            wave_Manager.wave_board.SetActive(true);
        }
    }

    public void DisableAllProvinceView()
    {
        foreach (var p in provinceViews)
        {
            p.OutClick();
        }
    }
}
