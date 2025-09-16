# 9.	Имеется N островов с заданными координатами. Координаты считаем двухмерными: (x, y) Необходимо соединить острова мостами так, чтобы их можно было обойти за минимальное время.
import random

# Число островов (можно менять)
N = 5

# Размер сетки (X на X)
X = 100

# Генерируем случайные координаты островов в квадрате [0, 100]x[0, 100]
points = []
i = 0
while i < N:
    x = random.randint(0, X)
    y = random.randint(0, X)
    points.append([x, y])
    i += 1

# Построим минимальное остовное дерево (MST) по евклидовому расстоянию (гипотенуза): sqrt(dx*dx + dy*dy).
# Используем алгоритм Прима с массивами, без приоритетной очереди для простоты.

visited = []
idx = 0
while idx < N:
    visited.append(False)
    idx += 1

key = []  # минимальная стоимость ребра до множества MST
parent = []  # откуда пришли
idx = 0
while idx < N:
    key.append(1e18)
    parent.append(-1)
    idx += 1

key[0] = 0.0
total_cost = 0.0

count = 0
while count < N:
    # Находим непосещенную вершину с минимальным key
    min_key = 1e18
    u = -1
    j = 0
    while j < N:
        if (not visited[j]) and key[j] < min_key:
            min_key = key[j]
            u = j
        j += 1

    visited[u] = True
    total_cost = total_cost + key[u]

    # Обновляем ключи соседей по евклидовому расстоянию
    j = 0
    while j < N:
        if not visited[j]:
            dx = points[u][0] - points[j][0]
            dy = points[u][1] - points[j][1]
            w = (dx*dx + dy*dy) ** 0.5
            if w < key[j]:
                key[j] = w
                parent[j] = u
        j += 1

    count += 1

# Собираем список ребер MST
edges = []
v = 1
while v < N:
    edges.append([parent[v], v])
    v += 1

print("Координаты островов:", points)
print("Суммарная длина мостов (евклидово):", total_cost)
print("Выбранные мосты (ребра MST):", edges)
