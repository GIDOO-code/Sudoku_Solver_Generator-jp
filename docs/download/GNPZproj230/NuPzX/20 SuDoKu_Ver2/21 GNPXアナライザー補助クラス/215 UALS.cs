using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections;

using GIDOO_space;

namespace GNPZ_sdk {
  #region UALS
    public class UALS: IComparable{
        static public Bit81[]    pHouseCells{ get{ return AnalyzerBaseV2.HouseCells; } }
        static public Bit81[]    pConnectedCells{ get{ return AnalyzerBaseV2.ConnectedCells; } }

        public int               ID;
        public readonly int      Size;  //セル数
        public readonly int      tfx;   //house番号
        public readonly int      FreeB; //ALSの要素数字
        public readonly int      Level; //FreeB数-セル数
        public readonly List<UCell> UCellLst = new List<UCell>();    //ALS構成セル
      
        public bool     singly;    //ALSLinkManで設定（ALSの構成が唯一）true：同じ構成でHouseが異なるALSの最初の登録
        public int      rcbDir;                                      //ALSの行列ブロックのビット表現
        public int      rcbRow{ get{ return (rcbDir&0x1FF); } }      //行のビット表現
        public int      rcbCol{ get{ return ((rcbDir>>9)&0x1FF); } } //列のビット表現
        public int      rcbBlk{ get{ return ((rcbDir>>18)&0x1FF); } }//ブロックのビット表現
        public Bit81    B81;                                         //ALSセル位置のビット表現
            
        //作業変数//(ALS Chainで用いる)
        public bool            LimitF=false;       
        public List<UALSPair>  ConnLst;
        public List<int>       LockedNoDir;
           
        public Bit81    rcbFilter;          //▼削除予定

        public UALS( int ID, int Size, int tfx, int FreeB, List<UCell> UCellLst ){
            this.ID        = ID;
            this.Size      = Size;
            this.tfx       = tfx;
            this.singly    = true;
            this.FreeB     = FreeB;
            this.Level     = FreeB.BitCount()-Size;
            this.B81       = new Bit81();
            this.rcbFilter = new Bit81();
            this.UCellLst  = UCellLst;
            this.LockedNoDir = null;

            UCellLst.ForEach( P =>{
                rcbDir |= ( (1<<(P.b+18)) | (1<<(P.c+9)) | (1<<(P.r)) );     
                rcbFilter |= pConnectedCells[P.rc];
                B81.BPSet(P.rc);
            } );
        }

        public override int GetHashCode(){ return (B81.GetHashCode() ^ FreeB*18401 ); }
        public int CompareTo( object obj ){
            UALS UB = obj as UALS;
            if( this.Level!=UB.Level ) return (this.Level-UB.Level);
            if( this.Size!=UB.Size )   return (this.Size-UB.Size);
            if( this.tfx!=UB.tfx )     return (this.tfx-UB.tfx);
            return (this.ID-UB.ID);
        }
/*
        public List<UCell> GetRestCells( int selB ){
            return pBDL.IEGetCellInHouse(tfx,selB).Where(P=>!B81.IsHit(P.rc)).ToList();
        }
*/
        public UGrCells SelectNoCells( int no ){
            int noB=1<<no;
            List<UCell> UCsS = UCellLst.FindAll(Q=>(Q.FreeB&noB)>0);
            UGrCells GCs = new UGrCells(tfx,no);
            GCs.Add(UCsS);
            return GCs;
        }
      
        public bool IsPureALS(){//fake ALSを判別(fake ALS:セル群の部分集合がLockedSet)
            if( Size<=2 ) return true;
            for( int sz=2; sz<Size-1; sz++ ){
                var cmb=new Combination(Size,sz);
                while(cmb.Successor()){
                    int fb=0;
                    for( int k=0; k<sz; k++ )  fb |= UCellLst[cmb.Cmb[k]].FreeB;
                    if( fb.BitCount()==sz ) return false;
                }
            }
            return true;
        }

        public override string ToString(){
            string po = "◇ UALS "+ID+" ◇  tfx:"+tfx +" Size:"+Size +" Level:"+Level;
            po += " NoB:" + FreeB.ToBitString(9) + "\r";
            po +=       "         B81 "+B81+"\r";
            for( int k=0; k<UCellLst.Count; k++){
                po += "------";
                int rcW = UCellLst[k].rc;
                po += " rc:" + ((rcW/9+1)*10+(rcW%9+1)).ToString();
                po += " FreeB:" + UCellLst[k].FreeB.ToBitString(9);
                po += " rcb:B" + (rcbBlk).ToBitString(9);
                po += " c" + rcbCol.ToBitString(9);
                po += " r" + rcbRow.ToBitString(9);
                po += " rcbFilter:" + rcbFilter.ToString();
                po += "\r";
            }
            return po;
        }

        public string ToStringRCN(){
            string st="";
            UCellLst.ForEach( p =>{  st += " r"+(p.r+1) + "c"+((p.c)+1); } );
            st = st.ToString_SameHouseComp()+" {#"+FreeB.ToBitStringN(9)+"}";
            return st;
        }
        public string ToStringRC(){
            string st="";
            UCellLst.ForEach( p =>{  st += " r"+(p.r+1) + "c"+((p.c)+1); } );
            st = st.ToString_SameHouseComp();
            return st;
        }
    }
  #endregion UALS
  
  //以下の2クラスは、いずれ GroupedLink に発展的にまとめられる（だろう）
  #region UALSPair
    public class UALSPair{
        public readonly UALS ALSpre;
        public readonly UALS ALSnxt;
        public readonly int  RCC;
        public readonly int  nRCC=-1; //no:0...8 (doubly の場合は個別にリンクを作る)
        public Bit81         rcUsed;
        public UALSPair( UALS ALSpre, UALS ALSnxt, int RCC, int nRCC, Bit81 rcB=null ){
            this.ALSpre=ALSpre; this.ALSnxt=ALSnxt; this.RCC=RCC; this.nRCC=nRCC;
            this.rcUsed = rcB?? (ALSpre.B81|ALSnxt.B81);
        }
        public  override bool Equals( object obj ){
            var A = obj as UALSPair;
            if( A.nRCC        !=nRCC )         return false;
            if( A.ALSpre.Size !=ALSpre.Size )  return false;
            if( A.ALSnxt.Size !=ALSnxt.Size )  return false; 
            if( A.ALSpre.FreeB!=ALSpre.FreeB ) return false;
            if( A.ALSnxt.FreeB!=ALSnxt.FreeB ) return false;
            if( A.ALSpre.B81!=ALSpre.B81 ) return false;
            if( A.ALSnxt.B81!=ALSnxt.B81 ) return false;
            return true;
        }
        public override Int32 GetHashCode(){ return base.GetHashCode(); }
    }
  #endregion UALSPair

  #region LinkCellAls
        public class LinkCellAls: IComparable{
        public readonly UCell UC;
        public readonly UALS  ALS;
        public readonly int   nRCC=-1; //no:0...8 (doubly の場合は個別にリンクを作る)
        public LinkCellAls( UCell UC, UALS ALS, int nRCC ){
            this.UC=UC; this.ALS=ALS; this.nRCC=nRCC;
        }
        public  override bool Equals( object obj ){
            var A = obj as LinkCellAls;
            return (this.ALS.ID==A.ALS.ID);
        }
        public override Int32 GetHashCode(){ return base.GetHashCode(); }
        public int CompareTo( object obj ){
            LinkCellAls A = obj as LinkCellAls;
            return (this.ALS.ID-A.ALS.ID);
        }

        public override string ToString(){
            string st="LinkCellAls nRCC:"+nRCC.ToBitString(9);
            st += " UCell:"+UC+"\rALS:"+ ALS;
            return st;
        }
    }
  #endregion LinkCellAls
}
