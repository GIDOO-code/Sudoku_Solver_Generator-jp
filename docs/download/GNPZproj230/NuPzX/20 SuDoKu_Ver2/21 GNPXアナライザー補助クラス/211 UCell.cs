﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using GIDOO_space;

namespace GNPZ_sdk{
    public class UCell{
        public object obj;

        public readonly int  rc;
        public readonly int  r;
        public readonly int  c;
        public readonly int  b;
        public int bx{ get{ return ((r%3)*3+(c%3)); } }

        public int      ErrorState; //0:無  1:確定 　8:ルール違反  9:解なし
        public int      No;         //>0:問題  =0:空き  <0:解
        public int      FreeB;
        public int      FreeBC{ get{ return FreeB.BitCount(); } }

        public int      FixedNo;  
        public int      CancelB;

        public List<EColor> ECrLst;
        
        public Color CellBgCr;     
        
        public bool Selected;       //作業用変数
        public int  Fixed=0;        //作業用変数
        public int  nx;             //作業用変数
        public int  FreeBD;         //degenerated

        public UCell( ){}

        public UCell( int rc, int No=0, int FreeB=0x1FF ){
            this.rc = rc;
            this.r  = rc/9;
            this.c  = rc%9;
            this.b  = rc/27*3+(rc%9)/3;
            this.No = No;
            this.FreeB = FreeB;

            this.ECrLst=null;
        }

        public void Reset_StepInfo(){
            ErrorState =0;
            CancelB  =0;
            FixedNo  =0;
            Selected =false;
            Fixed    =0;   

            this.ECrLst=null;
            CellBgCr = Colors.Black;       
        }

        public UCell Copy( ){
            UCell UCcpy=(UCell)this.MemberwiseClone();
            if( this.ECrLst!=null ){
                UCcpy.ECrLst=new List<EColor>();
                ECrLst.ForEach(p=>UCcpy.ECrLst.Add(p));
            }
            return UCcpy;
        }

        public void SetCellBgColor( Color CellBgCr ){ 
            if( ECrLst==null )  ECrLst=new List<EColor>();
            ECrLst.Add( new EColor(CellBgCr) );
        }

        public void SetNoBColor( int noB, Color cr ){
            if( ECrLst==null )  ECrLst=new List<EColor>();
            ECrLst.Add( new EColor(noB,cr) );
        }
        public void SetNoBColorRev( int noB, Color cr ){
            if( ECrLst==null )  ECrLst=new List<EColor>();
            ECrLst.Add( new EColor(noB,cr,cr) );
        }
        public void SetNoBBgColor( int noB, Color cr, Color crBg ){
            if( ECrLst==null )  ECrLst=new List<EColor>();
            ECrLst.Add( new EColor(noB,cr) );
            ECrLst.Add( new EColor(crBg) );
        }
        public void SetNoBBgColorRev( int noB, Color cr, Color crBg ){
            if( ECrLst==null )  ECrLst=new List<EColor>();
            ECrLst.Add( new EColor(noB,cr,cr) );
            ECrLst.Add( new EColor(crBg) );
        }
        public override string ToString(){
            string po = " UCell rc:"+rc+"["+((r+1)*10+(c+1)) +"]  no:"+No;
            po +=" FreeB:" + FreeB.ToBitString(9);
            po +=" CancelB:" + CancelB.ToBitString(9);
            return po;
        }
        public void ResetAnalysisResult(){
            CancelB  =0;
            FixedNo  =0;
            Selected =false;
            Fixed    =0; 
            this.ECrLst=null;
        }
    }
}