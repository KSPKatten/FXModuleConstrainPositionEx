using System;
using System.Collections.Generic;
using UnityEngine;

public class FXModuleConstrainPositionEx : FXModuleConstrainPosition
{
    private void Update()
    {
        if (trackingMode == FXModuleConstrainPosition.TrackMode.Update)
            TrackEx();
    }

    private void FixedUpdate()
    {
        if (trackingMode == FXModuleConstrainPosition.TrackMode.FixedUpdate)
            TrackEx();
    }

    private void LateUpdate()
    {
        if (trackingMode == FXModuleConstrainPosition.TrackMode.LateUpdate)
            TrackEx();
    }

    public void TrackEx()
    {
        if (ObjectsList == null)
        {
            Debug.Log(string.Format("[TrackEx]: ObjectsList == null"));
            return;
        }

        for (int i=0; i< ObjectsList.Count; i++)
        {
            ConstrainedObjectEx constrainedObject = ObjectsList[i];

            // Get the single target
            Transform targetTransform = base.part.FindModelTransform(constrainedObject.targetName);

            Vector3 position = new Vector3();
            Quaternion rotation = new Quaternion();
            for (int j=0; j<constrainedObject.movers.Count; j++)
            {
                ConstrainedObjectEx.ConstrainedObjectMover mover = constrainedObject.movers[j];

                float w = (float)mover.weight;
                Vector3 scale = new Vector3(w, w, w);

                Transform moverTransform = base.part.FindModelTransform(mover.transformName);

                // Add the scaled vector to the sum
                position += Vector3.Scale(moverTransform.position, scale);

                // Just use any rotation
                rotation = moverTransform.rotation;
            }

            if (matchPosition)
                targetTransform.position = position;

            /*
            if (matchRotation)
                transform.rotation = rotation;
            */
        }
    }

    public void SetupObjects(ConstrainedObjectEx co)
    {
        // Add all movers, assuming a list on the format "0.5*object1+0.5*object2"
        string[] moversStrings = co.moversName.Split('+');
        //co.movers.Clear();
        for (int i=0;i<moversStrings.Length;i++)
        {
            ConstrainedObjectEx.ConstrainedObjectMover mover = 
                new ConstrainedObjectEx.ConstrainedObjectMover();

            string[] strings = moversStrings[i].Split('*');
            if (strings.Length < 1 || strings.Length > 2)
            {
                Debug.LogError("[FXModuleConstrinPositionEx]: Every mover must be on the form '0.1*transformName' or 'transformName'");
                return;
            }
            mover.weight = 1.0;
            if (strings.Length == 1)
                mover.transformName = strings[0];
            else
            {
                mover.weight = double.Parse(strings[0]);
                mover.transformName = strings[1];
            }

            co.movers.Add(mover);
        }

        if (co.movers.Count >= 1)
            ObjectsList.Add(co);
    }

    /*
    public override void OnSave(ConfigNode node)
    {
        base.OnSave(node);
        foreach(ConstrainedObjectEx objectEx in ObjectsList)
            objectEx.Save(node.)
    }
    */

    public override void OnStart(StartState state)
    {
        base.OnStart(state);
    }

    public override void OnLoad(ConfigNode node)
    {
        if (ObjectsList == null)
            ObjectsList = new List<ConstrainedObjectEx>();

        trackingMode = (TrackMode)Enum.Parse(typeof(TrackMode), trackingModeString);
        if (node.HasNode("CONSTRAINFX"))    
        {
            ObjectsList.Clear();
            
            for(int i=0; i<node.nodes.Count; i++)
            {
                ConfigNode configNode = node.nodes[i];
                string name = configNode.name;
                if (name != null && name == "CONSTRAINFX")
                {
                    ConstrainedObjectEx constrainedObject = new ConstrainedObjectEx();
                    constrainedObject.Load(configNode);
                    SetupObjects(constrainedObject);
                }
            }
        }
    }
    
    // The fields of this class will persist from the cfg database to the instanced part.
    // However, it will instanced by reference - do not save any part-specific info in this
    // object!
    public class ConstrainedObjectEx : ScriptableObject
    {
        // A small class that keeps track of a transform and the weight assigned to that transform
        public class ConstrainedObjectMover
        {
            public double weight;
            public string transformName;
        }

        public ConstrainedObjectEx()
        {
            movers = new List<ConstrainedObjectMover>();
        }

        public void Load(ConfigNode node)
        {
            targetName = node.GetValue("targetName");
            moversName = node.GetValue("moversName");
        }

        public void Save(ConfigNode node)
        {
            node.AddValue("targetName", this.targetName);
            node.AddValue("moversName", this.moversName);
        }

        public string targetName;
        public string moversName;

        public List<ConstrainedObjectMover> movers;
    }

    public new List<ConstrainedObjectEx> ObjectsList;
}
