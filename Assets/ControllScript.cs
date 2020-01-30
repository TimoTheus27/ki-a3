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

    private int ghostAmount = 200;
    void Start() {
        for (int i = 0; i < ghostAmount; i++) {
            Ghostspawner.position = new Vector3(Random.Range(-49.0f, 49.0f), 0.5f, Random.Range(-100.0f, 100.0f));
            CreateGhost(Ghostspawner);
        }
    }

    void Update() {
        if (robotReady) {
            Debug.Log("New Update:");
            Debug.Log("Ghosts available: " + ghosts.Count);
            
            // move both
            GenerateRandomCommand();

            
            // töte schlechte ghosts
            var particleFilter = ParticleFilter();
            
            // töte alle ghosts out of map
            KillOutOfMapGhosts();
            
            // refill ghosts weighted
            refillGhosts(particleFilter);

            // abbruchbedingung
            //
            // if (CheckLocations()) {
            //     Debug.Log("final!");
            //     robot.CompareLocations(particleFilter[0].Item1.GetPosition(), particleFilter[0].Item1.GetRotation());
            //     return;
            // }
            // Debug.Log(robot.Scan());
            
            robotReady = false;
        }
    }

    private void GenerateRandomCommand() {
        int command = (int) (Random.value * 3f);
        switch (command) {
            case 0:
                robot.Move(Random.value * 10, robot.MaxForward);
                foreach (var ghost in ghosts) {
                    ghost.Move(Random.value * 10);
                }  
                return;
            case 1:
                robot.Rotate((Random.value - 0.5f) * 180f);
                foreach (var ghost in ghosts) {
                    ghost.Rotate((Random.value - 0.5f) * 180f);
                }  
                return;
            case 2:
                robot.Move(Random.value * 10, robot.MaxBackward);
                robot.Rotate((Random.value - 0.5f) * 180);
                foreach (var ghost in ghosts) {
                    ghost.Move(Random.value * 10);
                    ghost.Rotate((Random.value - 0.5f) * 180);
                }  
                return;
            default:
                robot.Rotate((Random.value - 0.5f) * 180);
                robot.Move(Random.value * 10, robot.MaxBackward);
                foreach (var ghost in ghosts) {
                    ghost.Rotate((Random.value - 0.5f) * 180);
                    ghost.Move(Random.value * 10);
                } 
                return;
        }
    }

    private void KillOutOfMapGhosts() {
        foreach (GhostController ghost in ghosts) {
            float x = ghost.GetPosition().x;
            float z = ghost.GetPosition().z;
            if (x < -48.6f || x > 48.6f || z < -105f || z > 105f) {
                //Debug.Log("Killing ghostie--- X : "+x+" Z : "+z);
                Destroy(ghost);
            }
        }
    }

    private List<Tuple<GhostController, float>> ParticleFilter() {
        
        // build list of wheightedGhosts
        List<Tuple<GhostController, float>> wheightedGhots = new List<Tuple<GhostController, float>>();
        foreach (var ghost in ghosts) {
            float distanceGhost = ghost.GetDistance();
            float distance = Math.Abs(robot.Scan() - distanceGhost);
            // Debug.Log("MAX: " + max);
            // Debug.Log("DISTANCE: " + distance);
            wheightedGhots.Add(new Tuple<GhostController, float>(ghost, distance));
        }

        //calculate mean
        List<float> differences = new List<float>();
        foreach (var ghost in ghosts) {
            float distanceGhost = ghost.GetDistance();
            // Debug.Log("Ghost: " + distanceGhost);
            // Debug.Log("Robot: " + robot.Scan());
            // Debug.Log("Distance: " + Mathf.Abs(distanceGhost - robot.Scan()));
            differences.Add( Math.Abs(robot.Scan() - distanceGhost));
        }
        float mean = differences.Sum() / ghostAmount;
        
        // kill all ghosts with differences in distance over mean
        foreach (var ghost in ghosts) {
            float distanceGhost = ghost.GetDistance();
            var distance = Math.Abs(robot.Scan() - distanceGhost);
            if (distance > mean) {
                Destroy(ghost);
            }
        }

        // sort top ghosts to start of array
        // Debug.Log("beforeSort: " + wheightedGhots[0]);
        wheightedGhots.Sort((x, y) => x.Item2.CompareTo(y.Item2));
        // Debug.Log("afterSort: " + wheightedGhots[0]);

        return wheightedGhots;
    }

    private void refillGhosts(List<Tuple<GhostController, float>> fillUpGhosts) {
        int ghostsCount = ghosts.Count;
        // Debug.Log("ghostCount: " + ghostsCount);
        int amountOfFillUpGhosts = ghostAmount - ghostsCount;
        // Debug.Log("amountOfFillUpGhosts: " + amountOfFillUpGhosts);
        for (int i = 0; i < amountOfFillUpGhosts; i++) {
            float x = fillUpGhosts[i].Item1.GetPosition().x;
            float z = fillUpGhosts[i].Item1.GetPosition().z;
            Ghostspawner.position = new Vector3(Random.Range(x - .5f , x + .5f), 0.5f, Random.Range(z - 1f, z + 1f));
            CreateGhost(Ghostspawner);        
        }
    }
    
    private Boolean CheckLocations() {
        float sumX = 0;
        float sumZ = 0;
        foreach (var ghost in ghosts) {
            sumX += ghost.GetPosition().x;
            sumZ += ghost.GetPosition().z;
        }

        float meanX = sumX / this.ghosts.Count;
        float meanY = sumZ / this.ghosts.Count;


        Debug.Log("AbbruchWert: " + meanX + meanY);
        
        
        if (meanX + meanY < 1) {
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