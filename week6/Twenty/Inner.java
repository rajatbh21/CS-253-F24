interface X {
  void f(int a);
}

public class Inner {
  public static void main(String[] args) {
    X x = new X() {
      public void f(int a) {
        System.out.println("hello");
      }
    };
    x.f(3);
    System.out.println("In the main method");
  }
}