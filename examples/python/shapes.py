import math


class Shape: # tradu[pt-br]:Forma
    def area(self): # tradu[pt-br]:area
        raise NotImplementedError("Subclasses must implement area()")

    def perimeter(self): # tradu[pt-br]:perimetro
        raise NotImplementedError("Subclasses must implement perimeter()")


class Circle(Shape): # tradu[pt-br]:Circulo
    def __init__(self, radius): # tradu[pt-br]:raio
        if radius <= 0:
            raise ValueError("Radius must be positive")
        self.radius = radius

    def area(self):
        return math.pi * self.radius ** 2

    def perimeter(self):
        return 2 * math.pi * self.radius

    def __str__(self):
        return f"Circle(radius={self.radius})"


class Rectangle(Shape): # tradu[pt-br]:Retangulo
    def __init__(self, width, height): # tradu[pt-br]:largura,height:altura
        if width <= 0 or height <= 0:
            raise ValueError("Dimensions must be positive")
        self.width = width
        self.height = height

    def area(self):
        return self.width * self.height

    def perimeter(self):
        return 2 * (self.width + self.height)

    def is_square(self): # tradu[pt-br]:eh_quadrado
        return self.width == self.height

    def __str__(self):
        return f"Rectangle({self.width}x{self.height})"


class Triangle(Shape): # tradu[pt-br]:Triangulo
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

    def is_equilateral(self): # tradu[pt-br]:eh_equilatero
        return self.a == self.b == self.c

    def __str__(self):
        return f"Triangle({self.a}, {self.b}, {self.c})"


def print_shapes_report(shapes): # tradu[pt-br]:imprimir_relatorio_formas,shapes:formas
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
