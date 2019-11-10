using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    partial class GNPZ_Analyzer{
        public bool GNP00_XChain(){
            CeLKMan.PrepareCellLink(1+2);    //strongLink(1),weakLink(2)����

            for( int no=0; no<9; no++ ){
                int noB=(1<<no);   

                List<int> LKRec=new List<int>();
                foreach( var CRL in _GetXChain(no,LKRec) ){
                    int rcS=CRL[0].ID;  //Bit81��ID���������ɗp����(�ϑ��I���p
                    
                    Bit81 ELM=(ConnectedCells[rcS]&CRL[1])-CRL[2]; //�N�_�֘A�Z������A���Z��
                    if( ELM.IsZero() ) continue;

                    foreach( var P in ELM.IEGetUCeNoB(pBDL,noB) ) P.CancelB=noB;

                    //===== X-Chain fond =====
                    SolCode = 2;
                    Bit81 LKRecB=SelectLink_XChain(LKRec,rcS,ELM,noB); //�֘A�����N�݂̂𒊏o                   
                    CRL[0]&=LKRecB; CRL[1]&=LKRecB;

                    Color Cr  = _ColorsLst[0];
                    Color Cr1 = Color.FromArgb(255,Cr.R,Cr.G,Cr.B); 
                    Color Cr2 = Color.FromArgb(120,Cr.R,Cr.G,Cr.B);    //(���x����)
                    foreach( var P in CRL[0].IEGetUCeNoB(pBDL,noB) ) P.SetNoBBgColor(noB,AttCr,Cr1);
                    foreach( var P in CRL[1].IEGetUCeNoB(pBDL,noB) ) P.SetNoBBgColor(noB,AttCr,Cr2);
                    pBDL[rcS].SetNoBBgColor(noB,AttCr,SolBkCr);
                    
                    Result = "X-Chain #"+(no+1);
                    if( SolInfoDsp )  ResultLong = Result;
                    if( !SnapSaveGP(true) )  return true;
                }
            }
            return false;
        }
        private Bit81 SelectLink_XChain( List<int> LKRec, int rcS, Bit81 ELM, int noB ){
            Bit81 LKRecB=new Bit81();
            foreach( var P in ELM.IEGetUCeNoB(pBDL,noB) ){
                int rcX=P.rc;
                LKRecB.BPSet(rcX);

                while(rcX!=rcS){
                    rcX=LKRec.Find(p=>(p&0xFF)==rcX);
                    if( rcX==0 ) break;
                    rcX=(rcX>>8);
                    LKRecB.BPSet(rcX);
                }
            }
            return LKRecB;
        }
               
        private IEnumerable<Bit81[]> _GetXChain( int no, List<int> LKRec ){
            Bit81 TBD = new Bit81(pBDL,(1<<no));

            int rcS;
            while( (rcS=TBD.FindFirstrc())>=0 ){
                TBD.BPReset(rcS);
                Bit81[] CRL=new Bit81[3];
                CRL[0]=new Bit81(); CRL[1]=new Bit81(rcS); CRL[2]=new Bit81();
                CRL[0].ID=rcS;
                Queue<int> rcQue=new Queue<int>();
                rcQue.Enqueue( (rcS<<1)|1 );    //(�ŏ��͋������N�Awhile���Ŕ��]) 

                LKRec.Clear();
                bool firstLK=true;
                while(rcQue.Count>0){
                    int rcX = rcQue.Dequeue();
                    int swF = 1-(rcX&1); 
                    int rc1 = (rcX>>1);
                    foreach( var P in CeLKMan.IEGetRcNoType(rc1,no,(swF+1)) ){
                        int rc2=P.rc2;
                        if( P.rc1==rcS || rc2==rcS )  continue;
                        if( (CRL[0]|CRL[1]).IsHit(rc2) ) continue;
                        CRL[swF].BPSet(rc2);
                        rcQue.Enqueue( (rc2<<1)|swF );
                        LKRec.Add( rc1<<8|rc2 );
                        if( firstLK ) CRL[2].BPSet(rc2); 
                    }
                    firstLK=false;
                }
                if( CRL[1].Count>0 ) yield return CRL;
            }
            yield break;
        }

    }
}