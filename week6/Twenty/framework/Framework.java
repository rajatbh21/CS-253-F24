import java.util.Scanner;
import java.net.MalformedURLException;
import java.net.URL;
import java.net.URLClassLoader;


class Main {
  public static void main(String[] args) {
    System.out.println("Enter class name to use:");
    Scanner in = new Scanner(System.in);
	  String name = in.nextLine();
    in.close();
	  System.out.println("Loading and instantiating " + name + "...");

    Class cls = null;
    URL classUrl = null;
    try {
      // Find classses in the given jar file
      // WRONG!!!!
      classUrl = new URL("file:///home/runner/ReflectionAndPlugins-RBB/deploy/app1.jar");
    } catch (Exception e) {
      e.printStackTrace();
    }
    URL[] classUrls = {classUrl};
    URLClassLoader cloader = new URLClassLoader(classUrls);
    try {
      cls = cloader.loadClass(name);
    } catch (Exception e) {
      e.printStackTrace();
    } 

    if (cls != null) {
	    try {
    		ISurprise adder = (ISurprise)cls.newInstance();
		    System.out.println("Surprise result: " + adder.surpriseOperation(4, 6));
	    } catch (Exception e) {
		    e.printStackTrace();
	    }

    }
  }
}