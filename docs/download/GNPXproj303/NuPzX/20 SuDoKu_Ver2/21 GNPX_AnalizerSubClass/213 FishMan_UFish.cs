using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Console;

using GIDOO_space;

namespace GNPZ_sdk{
    public class FishMan{
        private List<UCell>   pBDL;             
        private Bit81[]       pHouseCells;
        private Bit81[]       pConnectedCells;

        private int           sz;
        private int           no;

        private List<Bit81>   HBLst=new List<Bit81>();
        private bool          extFlag;

        public FishMan( AnalyzerBaseV2 AnB, int FMSize, int no, int sz, bool extFlag=false ){
            this.pBDL = AnB.pBDL;
            this.pHouseCells = AnalyzerBaseV2.HouseCells;
            this.pConnectedCells = AnalyzerBaseV2.ConnectedCells;
//            Bit81F.pConnectedCells = AnalyzerBaseV2.ConnectedCells;
            this.extFlag=extFlag;                     
            this.no=no; this.sz=sz;
            int noB=(1<<no);

            Bit81 Q, BPnoB=new Bit81(pBDL,noB);
            for( int tfx=0; tfx<FMSize; tfx++ ){ 
                Q = pHouseCells[tfx]&BPnoB;
                if( !Q.IsZero() && !HBLst.Contains(Q) ){ Q.ID=tfx; HBLst.Add(Q); }
//              if( !Q.IsZero() && !HBLst2.Contains(Q) ){ Q.ID=tfx; HBLst2.Add(new Bit81F(Q)); }
            }
            if( HBLst.Count<sz*2 ){ HBLst=null; return; }
        }

#if false
//BaseSet�ɏd�Ȃ�������ꍇ�A���̉��ǂ͎g���Ȃ� 
        public class Bit81F: Bit81{
            static public Bit81[]   pConnectedCells;
            public Bit81 Conn=null;
            public Bit81F( Bit81 rcB ){
                Conn=new Bit81();
                foreach(var rc in rcB.IEGet_rc()) Conn |= pConnectedCells[rc];
            }
        }
        private List<Bit81F>   HBLst2=new List<Bit81F>();

        public IEnumerable<UFish> IEGet_BaseSetEx( int BaseSel, bool EndoFlg=false ){
            if(HBLst2==null)  yield break;

            GeneralLogicGen.ChkBas1=0;
            GeneralLogicGen.ChkBas2=0;
            Bit81 Q;
            var prmBas=new Permutation(HBLst2.Count,sz);
            int nxt=int.MaxValue;
            while( prmBas.Successor(nxt) ){
                                    int chk1=++GeneralLogicGen.ChkBas1;
                int   usedLK=0;
                Bit81 HB81=new Bit81();
                Bit81 OHB81=new Bit81();
                for( int k=0; k<sz; k++ ){
                    nxt=k;
                    int nx=prmBas.Pnum[k];
                    if( ((1<<HBLst2[nx].ID)&BaseSel)==0 )  goto nxtCmb;
                    if( !(Q=HB81&HBLst2[nx]).IsZero() ){ //overlap
                        if(!EndoFlg)   goto nxtCmb; 
                        OHB81 |= Q;
                    }
                    usedLK |= 1<<HBLst2[nx].ID;  //house Number
                    HB81   |= HBLst2[nx];        //Bit81
                }
                if( extFlag && !IsLinked9(HB81) )  continue;
                                    int chk2=++GeneralLogicGen.ChkBas2;

                UFish UF = new UFish(no,sz,usedLK,HB81,OHB81);
                //if(sz>=3 && BaseSel==0x7FFFFFF) Debug_PattenPrint(UF);
                yield return UF;

              nxtCmb:
                continue;
            }
            yield break;
        }
#endif
        public IEnumerable<UFish> IEGet_BaseSet( int BaseSel, bool EndoFlg=false ){
            if(HBLst==null)  yield break;

            GeneralLogicGen.ChkBas1=0;
            GeneralLogicGen.ChkBas2=0;
            Bit81 Q;
            Combination cmbBas=new Combination(HBLst.Count,sz);
            int nxt=int.MaxValue;
            while( cmbBas.Successor(nxt) ){
                                    int chk1=++GeneralLogicGen.ChkBas1;
                int   usedLK=0;
                Bit81 HB81=new Bit81();
                Bit81 OHB81=new Bit81();
                for( int k=0; k<sz; k++ ){
                    nxt=k;
                    int nx=cmbBas.Cmb[k];
                    if( ((1<<HBLst[nx].ID)&BaseSel)==0 )  goto nxtCmb;
                    if( !(Q=HB81&HBLst[nx]).IsZero() ){ //overlap
                        if(!EndoFlg)   goto nxtCmb; 
                        OHB81 |= Q;
                    }
                    usedLK |= 1<<HBLst[nx].ID;  //house Number
                    HB81   |= HBLst[nx];        //Bit81
                }
                if( extFlag && !IsLinked9(HB81) )  continue;
                                    int chk2=++GeneralLogicGen.ChkBas2;

                UFish UF = new UFish(no,sz,usedLK,HB81,OHB81);
                //if(sz>=3 && BaseSel==0x7FFFFFF) Debug_PattenPrint(UF);
                yield return UF;

              nxtCmb:
                continue;
            }
            yield break;
        }
        public bool IsLinked9( Bit81 HB81 ){
            Bit81 Colored=new Bit81(), Processed=new Bit81();
            int rc0 = HB81.FindFirstrc();
            Colored.BPSet(rc0);
            while(true){
                Bit81 T = Colored-Processed;
                if( (rc0=T.FindFirstrc())<0 ) break;

                Processed.BPSet(rc0);
                Colored |= HB81&pConnectedCells[rc0];
                if( HB81.IsHit(rc0) ) Colored.BPSet(rc0);
            }
            return (HB81-Colored).IsZero();
        }
        private void Debug_PattenPrint( UFish UF ){
            WriteLine("no="+no+ " sz="+sz +"  BaseSet: " + UF.HouseB.HouseToString() );
            Bit81 BPnoB=new Bit81(pBDL,1<<no);
            string noST=" "+no.ToString();
            for( int r=0; r<9; r++ ){
                string st="";
                BPnoB.GetRowList(r).ForEach(p=>st+=(p==0? " .": noST));
                st+=" ";
                UF.BaseB81.GetRowList(r).ForEach(p=>st+=(p==0? " .": " B"));
                st+=" ";
                (BPnoB-UF.BaseB81).GetRowList(r).ForEach(p=>st+=(p==0? " .": " X"));
                WriteLine(st);
            }
        }

        public IEnumerable<UFish> IEGet_CoverSet( UFish BSet, int CoverSel, bool Finned, bool CannFlg=false ){
            if(HBLst==null)  yield break;

            List<Bit81> HCLst=new List<Bit81>();
            foreach( var P in HBLst.Where(q=>(BSet.HouseB&(1<<q.ID))==0) ){
                if( ((1<<P.ID)&CoverSel)==0 )  continue;
                if( BSet.BaseB81.IsHit(P) )  HCLst.Add(P);
            }
            if(HCLst.Count<sz) yield break;

            Bit81 Q;
            Combination cmbCov=new Combination(HCLst.Count,sz);
            int nxt=int.MaxValue;
            while( cmbCov.Successor(nxt) ){
                int chk1=++GeneralLogicGen.ChkCov1;

                int   usedLK=0;
                Bit81 HC81=new Bit81();
                Bit81 OHC81=new Bit81();
                for( int k=0; k<sz; k++ ){
                    nxt=k;
                    int nx=cmbCov.Cmb[k];                   
                    if( !(Q=HC81&HCLst[nx]).IsZero() ){ //overlap
                        if(!CannFlg)  goto nxtCmb;
                        OHC81 |= Q;
                    }
                    usedLK |= 1<<HCLst[nx].ID;  //house Number
                    HC81   |= HCLst[nx];        //Bit81
                }

                Bit81 FinB81=BSet.BaseB81-HC81;
                if( Finned!=(FinB81.Count>0) ) continue;
                UFish UF = new UFish(BSet,usedLK,HC81,FinB81,OHC81);
                //if(sz>=3 && CoverSel==0x7FFFFFF)  WriteLine("  CoverSet: " + UF.HouseC.HouseToString() ); //**********
                yield return UF;
                
              nxtCmb:
                continue;
            }
            yield break;
        }
    }

    public class UFish{
        public int      ID;
        public int      no;
        public int      sz;
        public Bit81    BaseB81=null;
        public Bit81    EndoFin=null;
        public int      HouseB=0;

        public UFish    BaseSet=null;
        public int      HouseC=0;
        public Bit81    CoverB81=null;
        public Bit81    FinB81=null;
        public Bit81    CannFin=null;

        public UFish(){ }

        public UFish( int no, int sz, int HouseB, Bit81 BaseB81, Bit81 EndoFin ){
            this.no=no;
            this.sz=sz;
            this.HouseB =HouseB;
            this.BaseB81=BaseB81;
            this.EndoFin=EndoFin;
        }
          
        public UFish( UFish BaseSet, int HouseC, Bit81 CoverB81, Bit81 FinB81, Bit81 CannFin ){
            this.BaseSet =BaseSet;
            this.HouseB  =BaseSet.HouseB;
            this.HouseC  =HouseC;
            this.CoverB81=CoverB81;
            this.FinB81  =FinB81;
            this.CannFin =CannFin;
        }
        public string ToString( string ttl ){
            string st = ttl + HouseB.HouseToString();
            return st;
        }
    }
}