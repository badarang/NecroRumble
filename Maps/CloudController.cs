using UnityEngine;

public class CloudController : MonoBehaviour
{
    private Player player;
    private float firstChildSpeed = 0.5f;
    private float secondChildSpeed = 0.5f;
    private int randomDirection;
    private Vector2 moveDirection;
    private Transform firstChild;
    private Transform secondChild;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        firstChildSpeed = Random.Range(0.5f, 1.5f);
        secondChildSpeed = Random.Range(0.5f, 1.5f);
        player = GameManager.Instance.GetPlayer();
        randomDirection = Random.Range(0, 8);
        moveDirection = GetDirection(randomDirection);
        firstChild = transform.GetChild(0);
        for (int i = 1; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
            {
                secondChild = transform.GetChild(i);
                break;
            }
        }
        
    }

    void Update()
    {
        MoveCloud();

        if (player != null)
        {
            float firstDistance = Vector2.Distance(firstChild.transform.position, player.transform.position);
            if (firstDistance > 40f)
            {
                firstChild.gameObject.SetActive(false);
            }
            float secondDistance = Vector2.Distance(secondChild.transform.position, player.transform.position);
            if (secondDistance > 40f)
            {
                secondChild.gameObject.SetActive(false);
            }
            if (firstChild.gameObject.activeSelf == false && secondChild.gameObject.activeSelf == false)
            {
                Destroy(gameObject);
            }
        }
    }

    void MoveCloud()
    {
        if (firstChild != null)
        {
            Vector2 firstMovement = moveDirection.normalized * firstChildSpeed * Time.deltaTime;
            firstChild.transform.position = new Vector3(firstChild.transform.position.x + firstMovement.x, firstChild.transform.position.y + firstMovement.y, firstChild.transform.position.z);
        }
        
        if (secondChild != null)
        {
            Vector2 secondMovement = moveDirection.normalized * secondChildSpeed * Time.deltaTime;
            secondChild.transform.position = new Vector3(secondChild.transform.position.x + secondMovement.x, secondChild.transform.position.y + secondMovement.y, secondChild.transform.position.z);
        }
    }

    Vector2 GetDirection(int direction)
    {
        switch (direction)
        {
            case 0: return new Vector2(1, 0);
            case 1: return new Vector2(1, 1);
            case 2: return new Vector2(0, 1);
            case 3: return new Vector2(-1, 1);
            case 4: return new Vector2(-1, 0);
            case 5: return new Vector2(-1, -1);
            case 6: return new Vector2(0, -1);
            case 7: return new Vector2(1, -1);
            default: return new Vector2(1, 0);
        }
    }
}