#pragma once

#ifndef LZSS_H
#define LZSS_H

//#define D_IMPORT

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>

#include <exception>

using namespace std;

typedef unsigned char uchar;
typedef unsigned long ulong;

//----------------------------------------------------------------------------
//��������� ������ � ������

typedef struct bfile
{
	FILE* file;
	uchar mask;
	int rack;
	int pacifier_counter;
}
BFILE;

#define PACIFIER_COUNT 2047

BFILE* OpenInputBFile(char* name);
BFILE* OpenOutputBFILE(char* name);
void  WriteBit(BFILE* bfile, int bit);
void  WriteBits(BFILE* bfile, ulong code, int count);
int   ReadBit(BFILE* bfile);
ulong ReadBits(BFILE* bfile, int bit_count);
void  CloseInputBFile(BFILE* bfile);
void  CloseOutputBFile(BFILE* bfile);

//----------------------------------------------------------------------------
// ������� �������� ������

void CompressFile(FILE* input, BFILE* output);
void ExpandFile(BFILE* input, FILE* output);

//----------------------------------------------------------------------------
// ������� ������ � ������� ������ ��� ��������� LZSS

void InitTree(int r);
void ContractNode(int old_node, int new_node);
void ReplaceNode(int old_node, int new_node);
int  FindNextNode(int node);
void DeleteString(int p);
int  AddString(int new_node, int* match_position);


//----------------------------------------------------------------------------
// ���������, ������������ ��� ������ LZSS

// ���������� ����� � ����, ���������� �������� ����������
#define INDEX_BITS  12
// ���������� ����� � ����, ���������� ����� ����������
#define LENGTH_BITS  4
// ����� ����������� ���� � ������
#define WINDOW_SIZE  ( 1 << INDEX_BITS)  //4096 ��� INDEX_BITS = 12
// ������������ ����� ����������
#define RAW_LOOK_AHEAD_SIZE  ( 1 << LENGTH_BITS)
// ���������, ������������ ������ �����������
#define BREAK_EVEN  (( 1 + INDEX_BITS + LENGTH_BITS) / 9)
// ������������ ����� ���������� � ������ ����, ��� �����
// 0 � 1 �� ������������
#define LOOK_AHEAD_SIZE  ( RAW_LOOK_AHEAD_SIZE + BREAK_EVEN)
// ������������ ���� - ������ ��������� ������
#define TREE_ROOT  WINDOW_SIZE
// ����������� ��������� ����� �����
#define END_OF_STREAM  0
// ����������� ����� ����
#define UNUSED  0
// �����, ����������� �������������� ������� ��� ������ �
// ��������� �������
#define MODULO(a)  ( (a) & (WINDOW_SIZE - 1) )

#pragma region IMPORT/EXPORT
#ifndef D_IMPORT

#define LZSS_LIB __declspec(dllexport)

#else

#define LZSS_LIB __declspec(dllimport)

#endif // !D_IMPORT
#pragma endregion

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

	void LZSS_LIB Compress(char* , char* );

	void LZSS_LIB DeCompress(char* , char* );

#ifdef __cplusplus
}
#endif // __cplusplus

#endif // !LIB_NAME_H