# Массив размерностью M состоит из случайных чисел из диапазона 1..N (M<N). Определить какие числа в нем отсутствуют.
import random

def find_missing_numbers(arr, N):
    missing = []
    num = 1
    while num <= N:
        found = False
        i = 0
        while i < len(arr):
            if arr[i] == num:
                found = True
                break
            i += 1
        if not found:
            missing.append(num)
        num += 1
    return missing
M = 5
N = 10
array = []
i = 0
while i < M:
    array.append(random.randint(1, N))
    i += 1
print("Массив:", array)
print("Отсутствующие числа:", find_missing_numbers(array, N))
