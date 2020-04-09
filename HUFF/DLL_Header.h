#pragma once
#ifndef HUFFMAN_H
#define HUFFMAN_H

//#define D_IMPORT

#include <cstdio>
#include <cstdlib>
#include <cctype>
#include <string>
#include <cstdarg>
#include <exception>


/*
 *   ���������, ������������ � ��������� �����������
 */
#define END_OF_STREAM	256    /* ������ ����� ������ */
#define ESCAPE				257    /* ������ ������ ESCAPE ������������������ */
#define SYMBOL_COUNT		258    /* ����������� ��������� ����������
					������� ������ (256+2 �������)*/
#define NODE_TABLE_COUNT	((SYMBOL_COUNT * 2) - 1)
#define ROOT_NODE				0
#define MAX_WEIGHT			0x8000 /* ��� �����, ��� ������� ����������
					��������������� ���� */
#define TRUE				1
#define FALSE				0

					/*
					 *  ��� ��������� ������ ����� ��� ���������� ������� � ������
					 */
typedef struct bit_file
{
	FILE* file;
	unsigned char mask;
	int rack;
	int pacifier_counter;
}
COMPRESSED_FILE;

/*
 *  ��� ��������� ������ ������������ ��� ������ � ������� �����������
 *  �������� ����������� ����������� � �������������
 */

struct node
{
	unsigned int weight;    /* ��� ���� */
	int parent;             /* ����� �������� � ������� ����� */
	int child_is_leaf;      /* ���� ����� (TRUE, ���� ����) */
	int child;
};

typedef struct tree
{
	int leaf[SYMBOL_COUNT];  /* ������ ������� ������ */
	int next_free_node;        /* ����� ����������
								  ���������� �������� ������� ������� */
	node nodes[NODE_TABLE_COUNT]; /* ������ ����� */
}
TREE;

/*
 *   ��������� �������
 */
void	fatal_error(const char* fmt);

/*
 *   ������� ���������� ������� � ������
 */
COMPRESSED_FILE* OpenInputCompressedFile(char* name);
COMPRESSED_FILE* OpenOutputCompressedFile(char* name);
void			 OutputBit(COMPRESSED_FILE*, int bit);
void			 OutputBits(COMPRESSED_FILE* bit_file, unsigned long code, int count);
int				 InputBit(COMPRESSED_FILE* bit_file);
unsigned long	 InputBits(COMPRESSED_FILE* bit_file, int bit_count);
void			 CloseInputCompressedFile(COMPRESSED_FILE* bit_file);
void			 CloseOutputCompressedFile(COMPRESSED_FILE* bit_file);

/*
 *   ���������� ���������� ����������� ��������
 */
void CompressFile(FILE* input, COMPRESSED_FILE* output);
void ExpandFile(COMPRESSED_FILE* input, FILE* output);
void InitializeTree(TREE* tree);
void EncodeSymbol(TREE* tree, unsigned int c, COMPRESSED_FILE* output);
int  DecodeSymbol(TREE* tree, COMPRESSED_FILE* input);
void UpdateModel(TREE* tree, int c);
void RebuildTree(TREE* tree);
void swap_nodes(TREE* tree, int i, int j);
void add_new_node(TREE* tree, int c);


#pragma region IMPORT/EXPORT
#ifndef D_IMPORT

#define HUFF_LIB __declspec(dllexport)

#else

#define HUFF_LIB __declspec(dllimport)

#endif // !D_IMPORT
#pragma endregion

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

	void HUFF_LIB Compress(char*, char*);

	void HUFF_LIB DeCompress(char*, char*);

#ifdef __cplusplus
}
#endif // __cplusplus

#endif // !HUFFMAN_H