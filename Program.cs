using System;
using System.Drawing;
//using System.Collections.Generic;
//using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using SharpCanvas;

namespace Mazer{
    public class Program{
        public static void Main(string[] args){
            Maze maze = new Maze(1000, 1000, 500, 500);
            maze.mode = 1;
            //maze.LoadFromImage("art.png");
            //maze.LoadFromImage("logo.png");
            //maze.LoadFromImage("braid200.png");
            //maze.LoadFromImage("normal.png");
            //maze.AntGenerateMap();
            maze.canvas.NextFrame(maze.Initialize);
        }
    }

    public class Maze{
        public Canvas canvas;
        public int[,] mazeMap;
        public int width;
        public int height;
        public int penSize = 2;
        public Vector2i startPoint;
        public Vector2i endPoint;
        public double delay = 0.1;
        public int movesDone = 0;
        public bool generated = false;
        public bool finished = false;
        public int mode = 0;
        double timer = 0.5;
        float squareWidth;
        float squareHeight;
        public MazeGenerator mazeGenerator;

        public Maze(int windowWidth, int windowHeight, int width, int height){
            canvas = new Canvas(windowWidth, windowHeight);
            mazeMap = new int[width, height];
            this.width = width;
            this.height = height;
            calculateSquareSize();
        }

        public void calculateSquareSize(){
            this.squareWidth = 100.0f / width;
            this.squareHeight = 100.0f / height;
        }

        public void Initialize(){
            canvas.updateAction = Update;
            canvas.BackgroundColor = Colorb.DarkGray;
            canvas.ImageCanvasMode();
            canvas.SetPenColor(Color.White);
            if(height<150 || width<150){
                penSize = 2;
            } else if(height<300 || width<300){
                penSize = 1;
            } else{
                penSize = 0;
            }
            canvas.SetPenSize(penSize);
            if(mode != -1){
                mazeGenerator = new MazeGenerator(this);
                if(mode == 0){
                    mazeGenerator.InitializePrim();
                    mazeGenerator.NextIterationPrim();
                } else if(mode == 1){
                    mazeGenerator.InitializeHunter();
                    mazeGenerator.NextIterationHunter();
                }
            } else{
                generated = true;
            }
        }

        public void AntGenerateMap(){
            mazeMap = new int[width, height];
            Console.WriteLine("Generating ant nest with width: " + (width) + " and height: " + (height) + ".");
            for (int i = 0; i < width; i++){
                mazeMap[i, 0] = 1;
                mazeMap[i, height-1] = 1;
            }
            for (int i = 0; i < height; i++){
                mazeMap[0, i] = 1;
                mazeMap[width-1, i] = 1;
            }
            Random r = new Random();
            startPoint = new Vector2i(r.Next(1, width-1), 0);
            endPoint = new Vector2i(r.Next(1, width-1), height-1);
            mazeMap[endPoint.x, endPoint.y] = 2;
            mazeMap[startPoint.x, startPoint.y] = 2;
            int[,] mapGen = (int[,]) mazeMap.Clone();
            int xPos = startPoint.x;
            int yPos = startPoint.y;
            int randX = 0;
            int targetX = r.Next(1, width-1);
            int targetY = r.Next(1, height-1);
            int end = (width+height)/16;
            while(true){
                if(randX > 0){xPos += 1; randX -= 1;}
                else if(randX < 0){xPos -= 1; randX += 1;}
                else if(targetY != yPos){yPos += targetY>yPos ? 1 : -1;}
                else if(targetX != xPos){xPos += targetX>xPos ? 1 : -1;}
                else{
                    if(end == 0){
                        break;
                    }
                    targetX = r.Next(1, width-1);
                    randX = r.Next(-4,5);
                    if(!(randX+xPos>1 && randX+xPos<width-1)){
                        randX = 0;
                    }
                    targetY = r.Next(1, height-1);
                    if(mapGen[targetX, targetY] != 0){
                        end -= 1;
                        if(end != 0){
                            targetX = r.Next(1, width-1);
                            targetY = r.Next(1, height-1);
                        } else{
                            targetX = endPoint.x;
                            targetY = endPoint.y-1;
                        }
                    }
                }
                if(mapGen[xPos+1, yPos] != 2) mapGen[xPos+1, yPos] = 1;
                if(mapGen[xPos-1, yPos] != 2) mapGen[xPos-1, yPos] = 1;
                if(mapGen[xPos, yPos+1] != 2) mapGen[xPos, yPos+1] = 1;
                if(mapGen[xPos, yPos-1] != 2) mapGen[xPos, yPos-1] = 1;
                int randTest = r.Next(-10,3);
                if(randTest >= 0){
                    randX = r.Next(-4,5);
                    if(!(randX+xPos>1 && randX+xPos<width-1)){
                        randX = 0;
                    }
                }
                mapGen[xPos, yPos] = 2;
            }
            for (int x = 0; x < width; x++){
                for (int y = 0; y < height; y++){
                    mazeMap[x, y] = mapGen[x, y] == 1 ? 1 : 0;
                }
            }
            mazeMap[startPoint.x, startPoint.y] = 2;
            Console.WriteLine("Done generating.");
        }

        public void LoadFromImage(string fileName){
            Bitmap b = (Bitmap) Bitmap.FromFile(fileName);
            b.RotateFlip(RotateFlipType.RotateNoneFlipY);
            width = b.Width;
            height = b.Height;
            Console.WriteLine("Loading maze with width: " + (width-1) + " and height: " + (height-1) + ".");
            squareWidth = 100.0f / width;
            squareHeight = 100.0f / height;
            mazeMap = new int[width, height];
            for (int x = 0; x < width; x++){
                for (int y = 0; y < height; y++){
                    mazeMap[x, y] = b.GetPixel(x, y).GetBrightness()>0.5f ? 0 : 1;
                }
            }
            for (int x = 0; x < width; x++){
                if(mazeMap[x, 0] == 0) startPoint = new Vector2i(x, 0);
                if(mazeMap[x, height-1] == 0) endPoint = new Vector2i(x, height-1);
            }
            //Console.WriteLine("End: " + endPoint.x + "x, " + endPoint.y + "y.");
            mazeMap[endPoint.x, endPoint.y] = 0;
            mazeMap[startPoint.x, startPoint.y] = 2;
        }

        public bool checkCompletion(){
            if(mazeMap[endPoint.x, endPoint.y] == 3){
                drawSquare(endPoint.x, endPoint.y, 4);
                return true;
            } else{
                return false;
            }
        }

        public bool BreadthFirst(){
            for (int z = 0; z < mazeGenerator.iterationsPerCall; z++){
                int num = 0;
                int[,] newMap = (int[,]) mazeMap.Clone();
                for (int i = 0; i < width; i++){
                    for (int j = 0; j < height; j++){
                        if(mazeMap[i, j] == 2){
                            num++;
                            newMap[i, j] = 3; drawSquare(i, j, 3);
                            if(i+1<height && mazeMap[i+1,j] == 0){newMap[i+1,j] = 2; drawSquare(i+1, j, 2);}
                            if(i-1>-1 && mazeMap[i-1,j] == 0){newMap[i-1,j] = 2; drawSquare(i-1, j, 2);}
                            if(j+1<height && mazeMap[i,j+1] == 0){newMap[i,j+1] = 2; drawSquare(i, j+1, 2);}
                            if(j-1>-1 && mazeMap[i,j-1] == 0){newMap[i,j-1] = 2; drawSquare(i, j-1, 2);}
                        }
                    }
                }
                if(num == 0){
                    Console.WriteLine("The maze doesnt have a solution.");
                    return false;
                }
                mazeMap = newMap;
            }
            return true;
        }

        public void drawMap(){
            for (int i = 0; i < width; i++){
                for (int j = 0; j < height; j++){
                    Square s = new Square(i*squareWidth, j*squareHeight, i*squareWidth+squareWidth, j*squareHeight+squareHeight);
                    if(mazeMap[i, j] == 0){
                        canvas.SetPenColor(Color.White);
                    } else if(mazeMap[i, j] == 1){
                        canvas.SetPenColor(Color.Black);
                    } else if(mazeMap[i, j] == 2){
                        canvas.SetPenColor(Color.Red);
                    } else if(mazeMap[i, j] == 3){
                        canvas.SetPenColor(Color.Blue);
                    }
                    canvas.PenCanvasDraw(s);
                    if(penSize != 0){
                        s.setFillMode(PolygonMode.Line);
                        canvas.SetPenColor(Color.Black);
                        canvas.PenCanvasDraw(s);
                    }
                }
            }
        }

        public void drawSquare(int x, int y, int value){
            Square s = new Square(x*squareWidth, y*squareHeight, x*squareWidth+squareWidth, y*squareHeight+squareHeight);
            if(value == 0){
                canvas.SetPenColor(Color.White);
            } else if(value == 1){
                canvas.SetPenColor(Color.Black);
            } else if(value == 2){
                canvas.SetPenColor(Color.Red);
            } else if(value == 3){
                canvas.SetPenColor(Color.Blue);
            } else if(value == 4){
                canvas.SetPenColor(Color.Yellow);
            }
            canvas.PenCanvasDraw(s);
            if(penSize != 0){
                s.setFillMode(PolygonMode.Line);
                canvas.SetPenColor(Color.Black);
                canvas.PenCanvasDraw(s);
            }
        }

        public void Update(){
            double delta = canvas.DeltaTime;
            timer -= delta;
            if(timer<0 && !finished){
                timer = delay;
                movesDone++;
                if(!generated){
                    if(mode == 0){
                        generated = mazeGenerator.NextIterationPrim();
                    } else if(mode == 1){
                        generated = mazeGenerator.NextIterationHunter();
                    }
                } else{
                    finished = !BreadthFirst();
                    drawMap();
                    if(!finished && checkCompletion()){
                        finished = true;
                        Console.WriteLine("Maze solved in " + movesDone + " moves.");
                    }
                }
            }
        }
    }
}