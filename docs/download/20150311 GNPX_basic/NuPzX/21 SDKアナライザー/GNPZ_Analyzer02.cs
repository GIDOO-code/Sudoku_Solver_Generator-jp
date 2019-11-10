using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    partial class GNPZ_Analyzer{
        public bool GNP00_LockedCandidate( ){
            int[] BNoBs = new int[9];
            for( int bx=0; bx<9; bx++ ) BNoBs[bx] =  pBDL.IEGet(18+bx,0X1FF).Aggregate(0,(Q,P)=>Q|P.FreeB );

            //==== Type-1 =====         
            for( int b0=0; b0<9; b0++ ){
                int BNoB;     
                if( (BNoB=BNoBs[b0])==0 ) continue;

                foreach( var no in BNoB.IEGet_BtoNo() ){
                    int noB=1<<no;
                    int rcBr=pBDL.IEGet(18+b0,noB).Aggregate(0,(Q,P)=>Q|(1<<P.r) );
                    if( rcBr.BitCount()==1 ){ //�s����
                        int r0=rcBr.BitToNum();
                        foreach( var P in pBDL.IEGet(0+r0,noB) ){ if( P.b!=b0 ){ SolCode=2; break; } }
                        if( SolCode>0 ){
                            foreach( var P in pBDL.IEGet(0+r0,noB) ){ 
                                if( P.b!=b0 ) P.CancelB=noB;
                                else          P.SetNoBBgColor(noB,AttCr,SolBkCr);
                            }
                            Result = "Locked Candidate B"+(b0+1)+" #"+(no+1);
                            if( SolInfoDsp ) ResultLong = Result;
                            if( !SnapSaveGP() )  return true;
                        }
                    }

                    int rcBc=pBDL.IEGet(18+b0,noB).Aggregate(0,(Q,P)=>Q|(1<<P.c) );
                    if( rcBc.BitCount()==1 ){  //�����
                        int c0=rcBc.BitToNum();
                        foreach( var P in pBDL.IEGet(9+c0,noB) ){ if( P.b!=b0 ){ SolCode=2; break; } }
                        if( SolCode>0 ){
                            foreach( var P in pBDL.IEGet(9+c0,noB) ){ 
                                if( P.b!=b0 ) P.CancelB=noB;
                                else          P.SetNoBBgColor(noB,AttCr,SolBkCr);
                            }
                            Result = "Locked Candidate B"+(b0+1)+" #"+(no+1);
                            if( SolInfoDsp ) ResultLong = Result;
                            if( !SnapSaveGP() )  return true;
                        }
                    }
                }
            }

            //==== Type-2 =====
            for( int b0=0; b0<9; b0++ ){ // �s����             b0:���ڃu���b�N
                int b1=b0/3*3+(b0+1)%3, b2=b0/3*3+(b0+2)%3; // b1,b2:�s�����̊֘A�u���b�N
                int BNoB = BNoBs[b0] & BNoBs[b1] & BNoBs[b2];
                if( BNoB==0 ) continue;�@//�֘A�u���b�N�ɐ����Ȃ�

                foreach( var no in BNoB.IEGet_BtoNo() ){�@//�֘A�u���b�N�ɂ��鐔���ɒ���
                    int noB=1<<no;
                    int rcBr0 = pBDL.IEGet(18+b0,noB).Aggregate(0,(Q,P)=> Q|(1<<P.r) );//���ڃu���b�N0�ɒ��ڐ����̂���s
                    if( rcBr0.BitCount()<=1 )  continue;
                    int rcBr1 = pBDL.IEGet(18+b1,noB).Aggregate(0,(Q,P)=> Q|(1<<P.r) );//�֘A�u���b�N1�ɒ��ڐ����̂���s
                    int rcBr2 = pBDL.IEGet(18+b2,noB).Aggregate(0,(Q,P)=> Q|(1<<P.r) );//�֘A�u���b�N2�ɒ��ڐ����̂���s

                    int rcBr12 = rcBr1|rcBr2;
                    int r0 = rcBr0.DifSet(rcBr12).BitToNum();
                    if( rcBr1>0 && rcBr2>0 && rcBr12.BitCount()==2 && r0>=0 ){ //����
                        SolCode=2;
                        foreach( var P in pBDL.IEGet(18+b0,noB) ){�@//���ڃu���b�N�̏��ݒ�
                            if( P.r!=r0 ) P.CancelB=noB;
                            else          P.SetNoBBgColor(noB,AttCr,SolBkCr);
                        }
                        foreach( var P in pBDL.IEGet(18+b1,noB) ) P.SetNoBBgColor(noB,AttCr,SolBkCr);//���u���b�N���ݒ�
                        foreach( var P in pBDL.IEGet(18+b2,noB) ) P.SetNoBBgColor(noB,AttCr,SolBkCr);
                        Result = "Locked Candidate B"+(b0+1)+" #"+(no+1);
                        if( SolInfoDsp ) ResultLong = Result;
                        if( !SnapSaveGP() )  return true;
                    }
                }
            }
            for( int b0=0; b0<9; b0++ ){   // �����    b0:���ڃu���b�N
                int b1=(b0+3)%9, b2=(b0+6)%9; //        b1,b2:������̊֘A�u���b�N
                int BNoB = BNoBs[b0] & BNoBs[b1] & BNoBs[b2];
                if( BNoB==0 ) continue;�@�@//�֘A�u���b�N�ɐ����Ȃ�

                foreach( var no in BNoB.IEGet_BtoNo() ){�@//�֘A�u���b�N�ɂ��鐔���ɒ���
                    int noB=1<<no;
                    int rcBc0 = pBDL.IEGet(18+b0,noB).Aggregate(0,(Q,P)=> Q|(1<<P.c) );//���ڃu���b�N0�ɒ��ڐ����̂����
                    if( rcBc0.BitCount()<=1 )  continue;
                    int rcBc1 = pBDL.IEGet(18+b1,noB).Aggregate(0,(Q,P)=> Q|(1<<P.c) );//�֘A�u���b�N1�ɒ��ڐ����̂����
                    int rcBc2 = pBDL.IEGet(18+b2,noB).Aggregate(0,(Q,P)=> Q|(1<<P.c) );//�֘A�u���b�N2�ɒ��ڐ����̂����

                    int rcBc12 = rcBc1|rcBc2;
                    int c0=(rcBc0.DifSet(rcBc12)).BitToNum();  //c0=(rcBc0&(rcBc12^0x1FF)).BitToNum();
                    if( rcBc1>0 && rcBc2>0 && rcBc12.BitCount()==2 && c0>=0 ){  //����
                        SolCode=2;
                        foreach( var P in pBDL.IEGet(18+b0,noB) ){//���ڃu���b�N�̏��ݒ�
                            if( P.c!=c0 ) P.CancelB=noB;
                            else          P.SetNoBBgColor(noB,AttCr,SolBkCr);
                        }
                        foreach( var P in pBDL.IEGet(18+b1,noB) ) P.SetNoBBgColor(noB,AttCr,SolBkCr);//���u���b�N���ݒ�
                        foreach( var P in pBDL.IEGet(18+b2,noB) ) P.SetNoBBgColor(noB,AttCr,SolBkCr);
                        Result = "Locked Candidate B"+(b0+1)+" #"+(no+1);
                        if( SolInfoDsp ) ResultLong = Result;
                        if( !SnapSaveGP() )  return true;
                    }
                }
            }

            return false;
        }
    }
}
