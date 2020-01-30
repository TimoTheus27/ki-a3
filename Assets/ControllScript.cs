using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ControllScript : MonoBehaviour {
    private List<GhostController> ghosts;
    private RobotController robot;
    private bool robotReady = false;
    public Transform Ghostspawner;

    void Start() {
        for (int i = 0; i < 100; i++) {
            Ghostspawner.position = new Vector3(Random.Range(-49.0f, 49.0f), 0.5f, Random.Range(-100.0f, 100.0f));
            CreateGhost(Ghostspawner);
        }
    }

    void Update() {
        if (robotReady) {
            
            // move both
            GenerateRandomCommand();
            GenerateRandomGhostCommand();

            // töte alle ghosts out of map
            KillOutOfMapGhosts();
            
            // töte schlechte ghosts
            ParticleFilter();
            
            // add new ghosts till amount is 200
            FillUpGhosts(100);

            // abbruchbedingung
            if (CheckLocations()) {
                Debug.Log("final!");
                return;
            }
            // robot.CompareLocations(ghosts[0].GetPosition(), ghosts[0].GetRotation());
            // Debug.Log(robot.Scan());
            
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
                robot.Rotate((Random.value - 0.5f) * 180);
                return;
            default:
                robot.Move(Random.value * 10, robot.MaxBackward);
                robot.Rotate((Random.value - 0.5f) * 180);
                return;
        }
    }
    
    private void GenerateRandomGhostCommand() {
        foreach (var ghost in ghosts) {
            int command = (int) (Random.value * 3f);
            switch (command) {
                case 0:
                    ghost.Move(Random.value * 10);
                    return;
                case 1:
                    ghost.Rotate((Random.value - 0.5f) * 180f);
                    return;
                case 2:
                    ghost.Move(Random.value * 10);
                    ghost.Rotate((Random.value - 0.5f) * 180);
                    return;
                default:
                    ghost.Rotate((Random.value - 0.5f) * 180);
                    ghost.Move(Random.value * 10);
                    return;
            }
        }
    }

    private void KillOutOfMapGhosts() {
        foreach (GhostController ghost in ghosts) {
            float x = ghost.GetPosition().x;
            float z = ghost.GetPosition().y;
            if (x < -48.6f || x > 48.6f || z < -105f || z > 105f) {
                //Debug.Log("Killing ghostie--- X : "+x+" Z : "+z);
                Destroy(ghost);
            }
        }
    }

    private void ParticleFilter() {
        List<float> differences = new List<float>();
        foreach (var ghost in ghosts) {
            differences.Add(Mathf.Abs(ghost.GetDistance() - robot.Scan()));
        }

        float mean = differences.Sum() / this.ghosts.Count;

        foreach (var ghost in ghosts) {
            float differenceToRobotDistance = Mathf.Abs(ghost.GetDistance() - robot.Scan());
            if (differenceToRobotDistance > mean) {
                Destroy(ghost);
            }
        }
    }

    private void FillUpGhosts(int ghostAmount) {
        int ghostsCount = ghosts.Count;
        Debug.Log("GhostCount after killing: " + ghostsCount);

        int amountOfFillUpGhosts = ghostAmount - ghostsCount;
        for (int i = 0; i < amountOfFillUpGhosts; i++) {
            Ghostspawner.position = new Vector3(Random.Range(-49.0f, 49.0f), 0.5f, Random.Range(-100.0f, 100.0f));
            CreateGhost(Ghostspawner);
        }
    }

    private Boolean CheckLocations() {
        float sumX = 0;
        float sumY = 0;
        foreach (var ghost in ghosts) {
            sumX += ghost.GetPosition().x;
            sumY += ghost.GetPosition().y;
        }

        Debug.Log("AbbruchWert: " + sumX + sumY);
        if (sumX + sumY < 10) {
            return true;
        }

        return false;
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