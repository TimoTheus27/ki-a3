using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllScript : MonoBehaviour {
    private List<GhostController> ghosts;
    private RobotController robot;
    private bool robotReady = false;
    public Transform Ghostspawner;

    void Start() {
        for (int i = 0; i < 200; i++) {
            CreateGhost(Ghostspawner);
        }
    }

    void Update() {
        if (robotReady) {
            foreach (GhostController ghost in ghosts) {
                ghost.Move(2);
                ghost.Rotate(Random.value * 90);
            }

            robot.CompareLocations(ghosts[0].GetPosition(), ghosts[0].GetRotation());
            Debug.Log(robot.Scan());
            float distance = robot.Scan();
            GenerateRandomCommand();
            robotReady = false;
        }
    }

    private void GenerateRandomCommand() {
        int command = (int) (Random.value * 3f);
        switch (command) {
            case 0:
                robot.Move(Random.value * 10, robot.MaxForward);
                return;
            case 1:
                robot.Rotate((Random.value - 0.5f) * 180f);
                return;
            case 2:
                robot.Move(Random.value * 10, robot.MaxBackward);
                return;
            default:
                robot.Rotate((Random.value - 0.5f) * 180);
                return;
        }
    }


    private void CreateGhost(Transform trans) {
        Instantiate(Ghost, trans.position, trans.rotation);
    }

    #region Stuff You Shouldn't Touch

    private static ControllScript self;
    public GameObject Ghost;

    void Awake() {
        if (self)
            Destroy(this);
        else
            self = this;
        ghosts = new List<GhostController>();
    }

    void OnDestroy() {
        self = null;
    }

    public static ControllScript GetInstance() {
        return self;
    }

    public void RegisterRobot(RobotController robot) {
        this.robot = robot;
    }

    public void RegisterGhost(GhostController ghost) {
        ghosts.Add(ghost);
    }

    public void DeRegisterGhost(GhostController ghost) {
        ghosts.Remove(ghost);
    }

    public void notifyRobotReady() {
        robotReady = true;
    }

    #endregion
}