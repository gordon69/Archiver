#include "DLL_Header.h"

//----------------------------------------------------------------------------
// Обработка фатальной ошибки при работе программы

void fatal_error(const char* str)
{
	throw std::exception(str);
}

//----------------------------------------------------------------------------
// Открытие файла для побитовой записи

BFILE* OpenOutputBFILE(char* name)
{
	BFILE* bfile;

	bfile = (BFILE*)calloc(1, sizeof(BFILE));
	if (0 == fopen_s(&bfile->file, name, "wb")) {
		bfile->rack = 0;
		bfile->mask = 0x80;
		bfile->pacifier_counter = 0;
		return bfile;
	}
	else {
		return nullptr;
	}
}

//----------------------------------------------------------------------------
// Открытие файла для побитового чтения

BFILE* OpenInputBFile(char* name)
{
	BFILE* bfile;

	bfile = (BFILE*)calloc(1, sizeof(BFILE));
	if (fopen_s(&bfile->file, name, "rb") == 0) {
		bfile->rack = 0;
		bfile->mask = 0x80;
		bfile->pacifier_counter = 0;
		return bfile;
	}
	else
	{
		return nullptr;
	}
}

//----------------------------------------------------------------------------
// Закрытие файла для побитовой записи

void CloseOutputBFile(BFILE* bfile)
{
	if (bfile->mask != 0x80)
		putc(bfile->rack, bfile->file);
	fclose(bfile->file);
	free((char*)bfile);
}

//----------------------------------------------------------------------------
// Закрытие файла для побитового чтения

void CloseInputBFile(BFILE* bfile)
{
	fclose(bfile->file);
	free((char*)bfile);
}

//----------------------------------------------------------------------------
// Вывод одного бита

void WriteBit(BFILE* bfile, int bit)
{
	if (bit)
		bfile->rack |= bfile->mask;
	bfile->mask >>= 1;
	if (bfile->mask == 0)
	{
		putc(bfile->rack, bfile->file);
		bfile->pacifier_counter++;
		bfile->rack = 0;
		bfile->mask = 0x80;
	}
}

//----------------------------------------------------------------------------
// Вывод нескольких битов

void WriteBits(BFILE* bfile, ulong code, int count)
{
	ulong mask;

	mask = 1L << (count - 1);
	while (mask != 0)
	{
		if (mask & code)
			bfile->rack |= bfile->mask;
		bfile->mask >>= 1;
		if (bfile->mask == 0)
		{
			putc(bfile->rack, bfile->file);
			bfile->pacifier_counter++;
			bfile->rack = 0;
			bfile->mask = 0x80;
		}
		mask >>= 1;
	}
}

//----------------------------------------------------------------------------
// Ввод одного бита

int ReadBit(BFILE* bfile)
{
	int value;

	if (bfile->mask == 0x80)
	{
		bfile->rack = getc(bfile->file);
		if (bfile->rack == EOF)
			fatal_error("Ошибка в процедуре ReadBit!\n");
		bfile->pacifier_counter++;
	}
	value = bfile->rack & bfile->mask;
	bfile->mask >>= 1;
	if (bfile->mask == 0)
		bfile->mask = 0x80;
	return (value ? 1 : 0);
}

//----------------------------------------------------------------------------
// Ввод нескольких битов

ulong ReadBits(BFILE* bfile, int bit_count)
{
	ulong mask;
	ulong return_value;

	mask = 1L << (bit_count - 1);
	return_value = 0;
	while (mask != 0)
	{
		if (bfile->mask == 0x80)
		{
			bfile->rack = getc(bfile->file);
			if (bfile->rack == EOF)
				fatal_error("Ошибка в процедуре ReadBits!\n");
			bfile->pacifier_counter++;
		}
		if (bfile->rack & bfile->mask)
			return_value |= mask;
		mask >>= 1;
		bfile->mask >>= 1;
		if (bfile->mask == 0)
			bfile->mask = 0x80;
	}
	return return_value;
}

//----------------------------------------------------------------------------
//исходный текст собственно алгоритма LZSS

// Скользящее окно
uchar window[WINDOW_SIZE];

// Двоичое дерево поиска совпадений
struct
{
	int parent;
	int smaller_child;
	int larger_child;
}
tree[WINDOW_SIZE + 1];

//----------------------------------------------------------------------------
// Инициализация двоичного дерева поиска

void InitTree(int r)
{
	tree[TREE_ROOT].larger_child = r;
	tree[r].parent = TREE_ROOT;
	tree[r].larger_child = UNUSED;
	tree[r].smaller_child = UNUSED;
}

//----------------------------------------------------------------------------
// Обработка удаления узла

void ContractNode(int old_node, int new_node)
{
	tree[new_node].parent = tree[old_node].parent;
	if (tree[tree[old_node].parent].larger_child == old_node)
		tree[tree[old_node].parent].larger_child = new_node;
	else
		tree[tree[old_node].parent].smaller_child = new_node;
	tree[old_node].parent = UNUSED;
}

//----------------------------------------------------------------------------
// Обработка удаления узла

void ReplaceNode(int old_node, int new_node)
{
	int parent;

	parent = tree[old_node].parent;
	if (tree[parent].smaller_child == old_node)
		tree[parent].smaller_child = new_node;
	else
		tree[parent].larger_child = new_node;
	tree[new_node] = tree[old_node];
	tree[tree[new_node].smaller_child].parent = new_node;
	tree[tree[new_node].larger_child].parent = new_node;
	tree[old_node].parent = UNUSED;
}

//----------------------------------------------------------------------------
// Поиск следующего меньшего, чем указанный, узла

int FindNextNode(int node)
{
	int next;

	next = tree[node].smaller_child;
	while (tree[next].larger_child != UNUSED)
		next = tree[next].larger_child;
	return next;
}

//----------------------------------------------------------------------------
// Удаление строки из двоичного дерева поиска

void DeleteString(int p)
{
	int replacement;

	if (tree[p].parent == UNUSED)
		return;
	if (tree[p].larger_child == UNUSED)
		ContractNode(p, tree[p].smaller_child);
	else
		if (tree[p].smaller_child == UNUSED)
			ContractNode(p, tree[p].larger_child);
		else
		{
			replacement = FindNextNode(p);
			DeleteString(replacement);
			ReplaceNode(p, replacement);
		}
}

//----------------------------------------------------------------------------
// Добавление строки в двоичное дерево поиска

int AddString(int new_node, int* match_pos)
{
	int i;
	int test_node;
	int delta;
	int match_len;
	int* child;

	if (new_node == END_OF_STREAM)
		return (0);

	test_node = tree[TREE_ROOT].larger_child;
	match_len = 0;

	for (;;)
	{
		for (i = 0; i < LOOK_AHEAD_SIZE; i++)
		{
			delta = window[MODULO(new_node + i)] -
				window[MODULO(test_node + i)];
			if (delta != 0)
				break;
		}

		if (i >= match_len)
		{
			match_len = i;
			*match_pos = test_node;
			if (match_len >= LOOK_AHEAD_SIZE)
			{
				ReplaceNode(test_node, new_node);
				return match_len;
			}
		}

		if (delta >= 0)
			child = &tree[test_node].larger_child;
		else
			child = &tree[test_node].smaller_child;

		if (*child == UNUSED)
		{
			*child = new_node;
			tree[new_node].parent = test_node;
			tree[new_node].larger_child = UNUSED;
			tree[new_node].smaller_child = UNUSED;
			return match_len;
		}
		test_node = *child;
	}
}

//----------------------------------------------------------------------------
// Процедура сжатия файла

void CompressFile(FILE* input, BFILE* output)
{
	for (int i = 0; i <= WINDOW_SIZE; i++) {
		tree[i].parent = UNUSED;
		tree[i].larger_child = UNUSED;
		tree[i].smaller_child = UNUSED;
	}

	int i, c, look_ahead_bytes, current_pos = 1, replace_count, match_len = 0, match_pos = 0;

	for (i = 0; i < LOOK_AHEAD_SIZE; i++)
	{
		if ((c = getc(input)) == EOF)
			break;
		window[current_pos + i] = (uchar)c;
	}

	look_ahead_bytes = i;

	InitTree(current_pos);

	while (look_ahead_bytes > 0)
	{
		if (match_len > look_ahead_bytes)
			match_len = look_ahead_bytes;
		if (match_len <= BREAK_EVEN)
		{
			replace_count = 1;
			WriteBit(output, 1);
			WriteBits(output, (ulong)window[current_pos], 8);
		}
		else
		{
			WriteBit(output, 0);
			WriteBits(output, (ulong)match_pos, INDEX_BITS);
			WriteBits(output, (ulong)(match_len - (BREAK_EVEN + 1)),
				LENGTH_BITS);
			replace_count = match_len;
		}

		for (i = 0; i < replace_count; i++)
		{
			DeleteString(MODULO(current_pos + LOOK_AHEAD_SIZE));
			if ((c = getc(input)) == EOF)
				look_ahead_bytes--;
			else
				window[MODULO(current_pos + LOOK_AHEAD_SIZE)] = (uchar)c;
			current_pos = MODULO(current_pos + 1);
			if (look_ahead_bytes)
				match_len = AddString(current_pos, &match_pos);
		}
	}

	WriteBit(output, 0);
	WriteBits(output, (ulong)END_OF_STREAM, INDEX_BITS);
}

//----------------------------------------------------------------------------
// Процедура декодирования сжатого файла

void ExpandFile(BFILE* input, FILE* output)
{
	int i;
	int current_pos;
	int c;
	int match_len;
	int match_pos;

	current_pos = 1;

	for (;;)
	{
		if (ReadBit(input))
		{
			c = (int)ReadBits(input, 8);
			putc(c, output);
			window[current_pos] = (uchar)c;
			current_pos = MODULO(current_pos + 1);
		}
		else
		{
			match_pos = (int)ReadBits(input, INDEX_BITS);
			if (match_pos == END_OF_STREAM)
				break;
			match_len = (int)ReadBits(input, LENGTH_BITS);
			match_len += BREAK_EVEN;
			for (i = 0; i <= match_len; i++)
			{
				c = window[MODULO(match_pos + i)];
				putc(c, output);
				window[current_pos] = (uchar)c;
				current_pos = MODULO(current_pos + 1);
			}
		}
	}
}

//Выносимые функции DLL

void Compress(char* InPath, char* OutPath) {
	FILE* Inp = nullptr;
	fopen_s(&Inp, InPath, "rb");
	BFILE* Out = OpenOutputBFILE(OutPath);

	CompressFile(Inp, Out);

	if (Inp != 0) {
		fclose(Inp);
	}
	CloseOutputBFile(Out);
}

void DeCompress(char* InPath, char* OutPath) {
	BFILE* Inp = OpenInputBFile(InPath);
	FILE* Out = nullptr;
	fopen_s(&Out, OutPath, "wb");

	ExpandFile(Inp, Out);

	CloseInputBFile(Inp);
	if (Out != 0) {
		fclose(Out);
	}
}