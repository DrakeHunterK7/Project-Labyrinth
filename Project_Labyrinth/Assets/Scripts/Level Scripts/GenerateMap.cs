using UnityEngine;
using UnityEngine.UIElements;

public class GenerateMap : MonoBehaviour
{
    [SerializeField]
    public int numberOfRooms;

    [SerializeField]
    public int gridDimension;

    [SerializeField]
    GameObject roomPrefab;

    // Start is called before the first frame update
    void Start()
    {
        GenerateRooms();
    }


    // We can change this function to instead load in prefab rooms later
    // Generates the rooms and assigns a random scale to them
    public void GenerateRooms()
    {
        //Room Dimension Range
        int xRange = 4;
        int zRange = 4;

        Vector3 roomPosition;
        Vector3 roomScale;

        // Clears rooms that were generated already
        ClearRooms();

        GameObject parentRoom = new GameObject("Rooms");
        GameObject room;

        // Add a random seed to the 
        Random.InitState((int)System.DateTime.Now.Ticks); ;

        for (int i = 0; i < numberOfRooms; i++)
        {
            roomScale = new Vector3(Random.Range(2, xRange), 1, Random.Range(2, zRange));

            // calculates position of room and then checks to see if it would be colliding with
            // rooms that already exist.
            do
            {
                // Calculates final position of room
                roomPosition = new Vector3(Random.Range(0, gridDimension), 0, Random.Range(0, gridDimension));
            }
            while (Physics.OverlapBox(roomPosition, roomScale + Vector3.one).Length > 0);

            // Instantiate room into scene
            room = Instantiate(roomPrefab, roomPosition, Quaternion.identity);

            // Rescales the 
            room.transform.localScale = roomScale;

            // Adds generated room to parent for easy deallocation
            room.transform.parent = parentRoom.transform;
        }
    }

    void DalaunayTriangulation()
    {

    }

    // Small function to help clean up scene when generating rooms
    public void ClearRooms()
    {
        GameObject removeObject = GameObject.Find("Rooms");

        if (removeObject != null)
        {
            DestroyImmediate(removeObject);
        }
    }
}
