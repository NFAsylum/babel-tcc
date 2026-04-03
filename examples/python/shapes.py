import math


class Shape: # tradu:Forma
    def area(self): # tradu:area
        raise NotImplementedError("Subclasses must implement area()")

    def perimeter(self): # tradu:perimetro
        raise NotImplementedError("Subclasses must implement perimeter()")


class Circle(Shape): # tradu:Circulo
    def __init__(self, radius): # tradu:raio
        if radius <= 0:
            raise ValueError("Radius must be positive")
        self.radius = radius

    def area(self):
        return math.pi * self.radius ** 2

    def perimeter(self):
        return 2 * math.pi * self.radius

    def __str__(self):
        return f"Circle(radius={self.radius})"


class Rectangle(Shape): # tradu:Retangulo
    def __init__(self, width, height): # tradu:largura,height:altura
        if width <= 0 or height <= 0:
            raise ValueError("Dimensions must be positive")
        self.width = width
        self.height = height

    def area(self):
        return self.width * self.height

    def perimeter(self):
        return 2 * (self.width + self.height)

    def is_square(self): # tradu:eh_quadrado
        return self.width == self.height

    def __str__(self):
        return f"Rectangle({self.width}x{self.height})"


class Triangle(Shape): # tradu:Triangulo
    def __init__(self, a, b, c):
        if a + b <= c or a + c <= b or b + c <= a:
            raise ValueError("Invalid triangle sides")
        self.a = a
        self.b = b
        self.c = c

    def area(self):
        s = self.perimeter() / 2
        return math.sqrt(s * (s - self.a) * (s - self.b) * (s - self.c))

    def perimeter(self):
        return self.a + self.b + self.c

    def is_equilateral(self): # tradu:eh_equilatero
        return self.a == self.b == self.c

    def __str__(self):
        return f"Triangle({self.a}, {self.b}, {self.c})"


def print_shapes_report(shapes): # tradu:imprimir_relatorio_formas,shapes:formas
    for shape in shapes:
        print(f"{shape}")
        print(f"  Area: {shape.area():.2f}")
        print(f"  Perimeter: {shape.perimeter():.2f}")


if __name__ == "__main__":
    shapes = [
        Circle(5),
        Rectangle(4, 6),
        Triangle(3, 4, 5),
    ]
    print_shapes_report(shapes)
