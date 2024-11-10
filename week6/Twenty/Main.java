// Reflection example

import java.lang.reflect.Method;
import java.lang.reflect.Field;
import java.lang.reflect.Constructor;
import java.util.Scanner;

interface IAdder<T> {
    T add(T a);
}

class AdderWithMemory implements IAdder<Integer> {
    private int lastValue = 0;

    public Integer add(Integer a) {
  	  lastValue = lastValue + a;
	    return lastValue;
    }

    public int getLastValue() {
	    return lastValue;
    }
}

class Rectangle implements IAdder<Rectangle>{
    private float length;
    private float width;
    public String name;

    public Rectangle(float l, float w, String n) {
  	  length = l;
	    width = w;
	    name = n;
    }

    public Rectangle add(Rectangle a) {
	    length += a.getLength();
	    width += a.getWidth();
	    return this;
    }

    public float getLength() { return length; }
    public float getWidth() { return width; }
}


class Main {
  public static void main(String[] args) {
    AdderWithMemory adder = new AdderWithMemory();
    System.out.println("Adder add result: " + adder.add(24));
    System.out.println("Adder last result: " + adder.getLastValue());
    System.out.println("Adder add result: " + adder.add(2));
    System.out.println("Adder last result: " + adder.getLastValue());

    Rectangle r1 = new Rectangle(3.0f, 5.0f, "r1");
    Rectangle r2 = new Rectangle(3.0f, 5.0f, "r2");
    System.out.println("Rectangle add result: " + r1.add(r2));
    System.out.println("Rectangle dimensions: " + r1.getLength() + ", " + r1.getWidth());


	  // ------------- Let's inspect ------------------------
    System.out.println("Enter a class to inspect: ");
    Scanner in =  new Scanner(System.in);
    String name = in.nextLine();
    System.out.println("Getting information about " + name);

    Class cls = null;
    try {
      cls = Class.forName(name);
    } catch (Exception e) {
      System.out.println("No such class: " + name);
    }

    if (cls != null) {
      Class[] interfaces = cls.getInterfaces();
      for (Class iface: interfaces)
        System.out.println("Interface name: " + iface.getName());

      Field[] fields = cls.getFields();
      for (Field f: fields)
        System.out.println("Found field: " + f.getName());

      Method[] methods = cls.getMethods();
      for (Method m: methods)
        System.out.println("Found method: " + m.getName());

	//------------ Let's invoke object r1 ------------------------
      //r1.add(r2);
      cls = r1.getClass();
      Method madd = null;
      try {
        madd = cls.getDeclaredMethod("add", Rectangle.class);
      } catch (Exception e) {
        System.out.println("Method not found add");
      }
      if (madd != null) {
          try {
            // r1.add(r2)
            madd.invoke(r1, r2);
          } catch (Exception e) {
            System.out.println("Illegal invocation");
          }
      }
      System.out.println("Rectangle dimensions: " + r1.getLength() + ", " + r1.getWidth());
    }
  }
}