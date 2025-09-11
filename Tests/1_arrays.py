# 9.	Массив размерностью M состоит из случайных чисел из диапазона 1..N (M<N). Определить какие числа в нем отсутствуют.
def find_missing_numbers(arr, N):
    """
    Функция возвращает список чисел от 1 до N, которых нет в массиве arr.
    """
    # Преобразуем входной массив в множество для быстрого поиска
    present = set(arr)
    # Формируем список отсутствующих чисел
    missing = [num for num in range(1, N + 1) if num not in present]
    return missing

# Пример использования
import random

M = 5
N = 10
# Генерируем массив из M случайных чисел в диапазоне 1..N
array = [random.randint(1, N) for _ in range(M)]
print("Массив:", array)

# Находим отсутствующие числа
absent_numbers = find_missing_numbers(array, N)
print("Отсутствующие числа:", absent_numbers)
