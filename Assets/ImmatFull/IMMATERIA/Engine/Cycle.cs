﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

namespace IMMATERIA
{
    [ExecuteInEditMode]
    public class Cycle : MonoBehaviour
    {

        [HideInInspector] public bool created = false;

        [HideInInspector] public bool begunGestation = false;
        [HideInInspector] public bool gestating = false;
        [HideInInspector] public bool gestated = false;

        [HideInInspector] public bool begunBirth = false;
        [HideInInspector] public bool birthing = false;
        [HideInInspector] public bool birthed = false;

        [HideInInspector] public bool begunLive = false;
        [HideInInspector] public bool living = false;
        [HideInInspector] public bool lived = false;

        [HideInInspector] public bool begunDeath = false;
        [HideInInspector] public bool dying = false;
        [HideInInspector] public bool died = false;

        [HideInInspector] public bool destroyed = true;


        public bool dontPropogate = false;
        public bool debug = false;
        public bool active = false;
        private bool oActive = false;

        public Cycle parent;
        public Data data;

        public List<Cycle> Cycles;


        public void DeleteAllCycle()
        {

        }

        /*
          Tying it up to the unity event system for enable and disable
        */
        /*void OnEnable(){
         //_Activate();
         DebugThis("enablio");
         if( living ){ _Activate(false); }
       }*/

        /*void OnDisable(){
          if( living ){ _Deactivate(false); }
        }*/



        /*

          Creation

        */

        public void Awake()
        {
            if (Cycles == null) { Cycles = new List<Cycle>(); }
        }
        public virtual void _Create() { DoCreate(); }
        public virtual void Create() { }

        protected void DoCreate()
        {

            float t = Time.time;

            //print("DOCREAS");



            //_Destroy();
            // SetStates();
            //  print( this );

            if (created) { DebugThis("Created Multiple Times"); }
            if (debug) { DebugThis("DoCreate"); }

            destroyed = false;
            created = true;

            Create();



            for (int i = Cycles.Count - 1; i >= 0; i--)
            {
                if (Cycles[i] == null)
                {
                    Cycles.RemoveAt(i);
                }
            }



            foreach (Cycle c in Cycles)
            {

                if (c == null)
                {
                    DebugThis("SOME CYCLE NULL");
                    // Cycles.Remove( c );
                }
                else
                {



                    CheckSelfCycle(c);


                    if (c.data == null) { c.data = data; }
                    if (data == null) { print("fuhhh"); }
                    c._Create();

                }
            }



        }

        /*

          Gestation

        */

        public virtual void _OnGestate() { DoGestate(); }
        public virtual void OnGestate() { }

        protected void DoGestate()
        {

            if (begunGestation) { DebugThis("On Gestate Multiple Times"); }
            if (debug) { DebugThis("DoGestate"); }
            begunGestation = true;

            _Bind();

            OnGestate();
            foreach (Cycle c in Cycles)
            {

                CheckSelfCycle(c);
                c._OnGestate();
            }

            gestating = true;

        }

        public virtual void _Bind() { Bind(); }
        public virtual void Bind() { }

        public virtual void _WhileGestating(float v) { DoGestating(v); }
        public virtual void WhileGestating(float v) { }

        protected void DoGestating(float v)
        {
            WhileGestating(v);
            foreach (Cycle c in Cycles)
            {

                CheckSelfCycle(c);
                c._WhileGestating(v);
            }
        }



        public virtual void _OnGestated() { DoGestated(); }
        public virtual void OnGestated() { }

        protected void DoGestated()
        {

            if (gestated) { DebugThis("On Gestated Multiple Times"); }
            if (debug) { DebugThis("DoGestated"); }
            gestating = false;
            OnGestated();
            foreach (Cycle c in Cycles)
            {

                CheckSelfCycle(c);
                c._OnGestated();
            }
            gestated = true;

        }



        /*

          Birth

        */


        public virtual void _OnBirth() { DoBirth(); }
        public virtual void OnBirth() { }

        protected void DoBirth()
        {
            if (begunBirth) { DebugThis("begunBirth multiple times"); }
            if (debug) { DebugThis("DoBirth"); }
            begunBirth = true;
            OnBirth();
            foreach (Cycle c in Cycles)
            {

                CheckSelfCycle(c);
                c._OnBirth();
            }
            birthing = true;
        }



        public virtual void _WhileBirthing(float v) { DoBirthing(v); }
        public virtual void WhileBirthing(float v) { }

        protected void DoBirthing(float v)
        {
            WhileBirthing(v);
            foreach (Cycle c in Cycles)
            {

                CheckSelfCycle(c);
                c._WhileBirthing(v);
            }
        }


        public virtual void _OnBirthed() { DoBirthed(); }
        public virtual void OnBirthed() { }

        protected void DoBirthed()
        {
            if (birthed) { DebugThis("On Birthed Multiple Times"); }
            birthing = false;
            OnBirthed();
            foreach (Cycle c in Cycles)
            {

                CheckSelfCycle(c);
                c._OnBirthed();
            }
            birthed = true;
        }


        /*

          LIVE

        */

        public virtual void _OnLive() { DoLive(); }
        public virtual void OnLive() { }

        protected void DoLive()
        {
            if (living) { DebugThis("BegunLive Multiple Times"); }
            begunLive = true;
            OnLive();
            foreach (Cycle c in Cycles)
            {

                CheckSelfCycle(c);
                c._OnLive();
            }
            living = true;
        }

        public virtual void _WhileLiving(float v) { DoLiving(v); }
        public virtual void WhileLiving(float v) { }

        protected void DoLiving(float v)
        {

            if (active)
            {
                WhileLiving(v);

                foreach (Cycle c in Cycles)
                {
                    CheckSelfCycle(c);
                    c._WhileLiving(v);
                }
            }

            if (oActive == false && active == true)
            {
                _Activate();
            }
            else if (oActive == true && active == false)
            {
                _Deactivate();
            }

            oActive = active;

        }

        public virtual void _OnLived() { DoLived(); }
        public virtual void OnLived() { }

        void DoLived()
        {
            if (lived) { DebugThis("on lived Multiple Times"); }
            living = false;
            OnLived();
            foreach (Cycle c in Cycles)
            {

                CheckSelfCycle(c);
                c._OnLived();
            }
            lived = true;
        }



        /*

          DEATH

        */


        public virtual void _OnDie() { DoDie(); }
        public virtual void OnDie() { }

        protected void DoDie()
        {
            if (begunDeath) { DebugThis("On Die Multiple Times"); }
            begunDeath = true;
            OnDie();
            foreach (Cycle c in Cycles)
            {

                CheckSelfCycle(c);
                c._OnDie();
            }
            dying = true;
        }

        public virtual void _WhileDying(float v) { DoDying(v); }
        public virtual void WhileDying(float v) { }

        protected void DoDying(float v)
        {

            WhileDying(v);
            foreach (Cycle c in Cycles)
            {

                CheckSelfCycle(c);
                c._WhileDying(v);
            }
        }

        public virtual void _OnDied() { DoDied(); }
        public virtual void OnDied() { }

        protected void DoDied()
        {
            if (died) { DebugThis("On Died Multiple Times"); }
            dying = false;
            OnDied();
            foreach (Cycle c in Cycles)
            {

                CheckSelfCycle(c);
                c._OnDied();
            }
            died = true;
        }



        /*
            Destroy
        */

        public virtual void _Destroy() { DoDestroy(); }
        public virtual void Destroy() { }

        protected void DoDestroy()
        {
            //   print( Cycles );
            for (int i = Cycles.Count - 1; i >= 0; i--)
            {
                if (Cycles[i] == null)
                {
                    Cycles.RemoveAt(i);
                }
            }

            foreach (Cycle c in Cycles)
            {

                if (c == null)
                {

                    DebugThis("Some Cycle Null");
                }
                else
                {

                    if (c.data == null) { c.data = data; }
                    if (data == null) { print("fuhhh"); }
                    CheckSelfCycle(c);
                    c._Destroy();

                }
            }


            Destroy();

            SetStates();


        }



        /*

          Activate Deactivate

        */

        public virtual void _Activate()
        {
            Activate();
            if (!dontPropogate)
            {
                foreach (Cycle c in Cycles)
                {

                    CheckSelfCycle(c);
                    c._Activate();
                }
            }
            active = true;
        }


        // Making it so we can activate this object and not every one of the children!
        public virtual void _Activate(bool propogate)
        {
            Activate();

            if (propogate)
            {
                foreach (Cycle c in Cycles)
                {

                    CheckSelfCycle(c);
                    c._Activate();
                }
            }
            active = true;
        }


        public virtual void Activate() { }


        public virtual void _Deactivate()
        {
            Deactivate();
            if (!dontPropogate)
            {
                foreach (Cycle c in Cycles)
                {
                    CheckSelfCycle(c);
                    c._Deactivate();
                }
            }
            active = false;
        }


        public virtual void _Deactivate(bool propogate)
        {
            Deactivate();

            if (propogate)
            {
                foreach (Cycle c in Cycles)
                {
                    CheckSelfCycle(c);
                    c._Deactivate();
                }
            }
            active = false;
        }

        public virtual void Deactivate() { }


        void SetStates()
        {

            created = false;
            begunGestation = false;
            gestating = false;
            gestated = false;
            begunBirth = false;
            birthed = false;
            birthing = false;
            begunLive = false;
            living = false;
            lived = false;
            begunDeath = false;
            dying = false;
            died = false;
            destroyed = true;
        }

        /*
              DEBUG
        */

        public virtual void _WhileDebug() { DoDebug(); }
        public virtual void WhileDebug() { }

        protected void DoDebug()
        {

            if (debug) { WhileDebug(); }

            foreach (Cycle c in Cycles)
            {
                CheckSelfCycle(c);
                // print(c);
                c._WhileDebug();

            }

        }


        public void SafeInsert(Cycle c2)
        {

            bool can = true;

            foreach (Cycle c in Cycles)
            {
                if (c == c2) can = false;
            }


            if (can) Cycles.Insert(Cycles.Count, c2);

        }

        public void SafePrepend(Cycle c2)
        {

            bool can = true;

            foreach (Cycle c in Cycles)
            {
                if (c == c2) can = false;
            }


            if (can) Cycles.Insert(0, c2);

        }


        public void Reset()
        {
            created = false;
            begunGestation = false;
            gestating = false;
            gestated = false;
            begunBirth = false;
            birthing = false;
            birthed = false;
            begunLive = false;
            living = false;
            lived = false;
            begunDeath = false;
            dying = false;
            died = false;
            destroyed = true;


        }



        public void SpinUp()
        {
            _Destroy();
            Reset();
            _Create();
            _OnGestate();
            _OnGestated();
            _OnBirth();
            _OnBirthed();
        }

        public void SpinDown()
        {
            _Destroy();
            Reset();
        }



        /*
          Helpers
        */
        public void DebugThis(string s)
        {
            Debug.Log("Object Name : " + this.gameObject.name + "     || Script Name : " + this.GetType() + "     || Message: " + s, this.gameObject);
        }

        public void CheckSelfCycle(Cycle c)
        {
            if (c == this)
            {
                Debug.LogError("YOU CYCLED YOURSELF!" + c);

                Cycles.Remove(c);
            }
        }


        public void SafeGet(ref dynamic toGet, Type myType)
        {

#if UNITY_EDITOR
    print( toGet );
    if( toGet == null ){
      toGet = GetComponent(myType);
      if( toGet == null ){
        DebugThis("Couldn't Auto Assign " + toGet );
      }else{
        print( toGet );
        EditorGUIUtility.PingObject(toGet);
        DebugThis("Auto-Assigned " + toGet );
      }
    }

#endif

        }



        public void JumpStart(Cycle c)
        {
            if (data != null)
            {
                c.data = data;
            }
            else
            {
                DebugThis("NO DATA BAD BAD");
            }
            SafeInsert(c);

            c._Destroy();
            c.Reset();
            c._Create();
            c._OnGestate();
            c._OnGestated();
            c._OnBirth();
            c._OnBirthed();
            c._Activate();

        }


        // Add an array of cycles, to this cycle
        // making sure that they get executed in
        // order correctly
        public void JumpStart(Cycle[] c)
        {


            for (int i = 0; i < c.Length; i++)
            {
                if (data != null)
                {
                    c[i].data = data;
                }
                else
                {
                    DebugThis("NO DATA BAD BAD");
                }
            }


            for (int i = 0; i < c.Length; i++)
            {
                SafeInsert(c[i]);
            }

            for (int i = 0; i < c.Length; i++)
            {
                c[i]._Destroy();
            }

            for (int i = 0; i < c.Length; i++)
            {
                c[i].Reset();
            }

            for (int i = 0; i < c.Length; i++)
            {
                c[i]._Create();
            }

            for (int i = 0; i < c.Length; i++)
            {
                c[i]._OnGestate();
            }

            for (int i = 0; i < c.Length; i++)
            {
                c[i]._OnGestated();
            }

            for (int i = 0; i < c.Length; i++)
            {
                c[i]._OnBirth();
            }

            for (int i = 0; i < c.Length; i++)
            {
                c[i]._OnBirthed();
            }


            for (int i = 0; i < c.Length; i++)
            {
                c[i]._Activate();
            }

        }


        public void JumpDeath(Cycle c)
        {

            if (data != null)
            {
                c.data = data;
            }
            else
            {
                DebugThis("NO DATA BAD BAD");
            }
            c._Deactivate();
            c._OnDie();
            c._OnDied();
            c._Destroy();
            Cycles.Remove(c);
        }


        public void JumpDeath(Cycle[] c)
        {
            for (int i = 0; i < c.Length; i++)
            {
                if (data != null)
                {
                    if (c[i] != null)
                    {
                        c[i].data = data;
                    }
                    else
                    {
                        DebugThis("NO CYCLE REF BAD BAD");
                    }
                }
                else
                {
                    DebugThis("NO DATA BAD BAD");
                }
            }
            for (int i = 0; i < c.Length; i++)
            {
                c[i]._Deactivate();
            }


            for (int i = 0; i < c.Length; i++)
            {
                c[i]._OnDie();
            }
            for (int i = 0; i < c.Length; i++) { c[i]._OnDied(); }
            for (int i = 0; i < c.Length; i++) { c[i]._Destroy(); }
            for (int i = 0; i < c.Length; i++) { Cycles.Remove(c[i]); }
        }


        public void PrintParentStructure()
        {

            bool hasParent = true;

            string parentString = "";

            Cycle parentCycle = this;
            while (hasParent == true)
            {

                if (parentCycle.parent != null)
                {
                    parentString += " || " + parentCycle.gameObject + ":" + parentCycle.GetType();
                    parentCycle = parentCycle.parent;
                }
                else
                {
                    hasParent = false;
                }

            }



            DebugThis(parentString);

        }



    }
}