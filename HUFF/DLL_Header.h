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
 *   Константы, используемые в алгоритме кодирования
 */
#define END_OF_STREAM	256    /* Маркер конца потока */
#define ESCAPE				257    /* Маркер начала ESCAPE последовательности */
#define SYMBOL_COUNT		258    /* Максимально возможное количество
					листьев дерева (256+2 маркера)*/
#define NODE_TABLE_COUNT	((SYMBOL_COUNT * 2) - 1)
#define ROOT_NODE				0
#define MAX_WEIGHT			0x8000 /* Вес корня, при котором начинается
					масштабирование веса */
#define TRUE				1
#define FALSE				0

					/*
					 *  Эта структура данных нужна для побитового доступа к файлам
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
 *  Эта структура данных используется для работы с деревом кодирования
 *  Хаффмена процедурами кодирования и декодирования
 */

struct node
{
	unsigned int weight;    /* Вес узла */
	int parent;             /* Номер родителя в массиве узлов */
	int child_is_leaf;      /* Флаг листа (TRUE, если лист) */
	int child;
};

typedef struct tree
{
	int leaf[SYMBOL_COUNT];  /* Массив листьев дерева */
	int next_free_node;        /* Номер следующего
								  свободного элемента массива листьев */
	node nodes[NODE_TABLE_COUNT]; /* Массив узлов */
}
TREE;

/*
 *   Сервисные функции
 */
void	fatal_error(const char* fmt);

/*
 *   Функции побитового доступа к файлам
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
 *   Собственно адаптивное кодирование Хаффмена
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