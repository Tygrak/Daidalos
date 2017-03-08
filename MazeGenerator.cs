using System;
using System.Collections.Generic;
using SharpCanvas;

namespace Mazer{
    public class MazeGenerator{
        Maze m;
        Random r;
        List<Vector2i> walls;
        int x = 0;
        int y = 0;
        public int iterationsPerCall = 1;
        bool hunt = false;
        public MazeGenerator(Maze maze){
            this.m = maze;
            if(m.mode == 0){
                if(m.width%2==0) m.width = m.width+1;
                if(m.height%2==0) m.height = m.height+1;
                m.calculateSquareSize();
            }
            m.mazeMap = new int[m.width, m.height];
            Console.WriteLine("Generating maze with width: " + (m.width) + " and height: " + (m.height) + ".");
            iterationsPerCall = (m.width*m.height)/450;
            if(iterationsPerCall<1) iterationsPerCall = 1;
            for (int x = 0; x < m.width; x++){
                for (int y = 0; y < m.height; y++){
                    m.mazeMap[x, y] = 1;
                }
            }
            r = new Random();
            m.startPoint = new Vector2i(r.Next(1, m.width-1), 0);
            if(m.startPoint.x % 2 == 0){
                m.startPoint.x += m.startPoint.x+1<m.width-1 ? 1 : m.startPoint.x-1>0 ? -1 : 0;
            }
            m.endPoint = new Vector2i(r.Next(1, m.width-1), m.height-1);
        }

        public void InitializePrim(){
            walls = new List<Vector2i>();
            m.mazeMap[m.startPoint.x, m.startPoint.y] = 0;
            m.mazeMap[m.startPoint.x, m.startPoint.y+1] = 0;
            addNeighbours(m.startPoint.x, m.startPoint.y+1);
        }

        public void InitializeHunter(){
            m.mazeMap[m.startPoint.x, m.startPoint.y] = 0;
            m.mazeMap[m.startPoint.x, m.startPoint.y+1] = 0;
            x = m.startPoint.x;
            y = m.startPoint.y+1;
        }

        public bool NextIterationHunter(){
            for (int i = 0; i < iterationsPerCall; i++){
                m.mazeMap[x, y] = 0;
                if(unVisitedNeighbourNum(x, y) > 0 && !hunt){
                    hunt = !randomWalk(x, y);
                } else if(!hunt){
                    hunt = true;
                } else{
                    hunt = false;
                    if(!huntSquare()){
                        placeExit();
                        m.mazeMap[m.startPoint.x, m.startPoint.y] = 2;
                        m.drawMap();
                        Console.WriteLine("Done generating.");
                        return true;
                    }
                }
            }
            m.drawMap();
            return false;
        }

        public bool NextIterationPrim(){
            for (int i = 0; i < iterationsPerCall; i++){
                if(walls.Count<1){
                    placeExit();
                    m.mazeMap[m.startPoint.x, m.startPoint.y] = 2;
                    m.drawMap();
                    Console.WriteLine("Done generating.");
                    return true;
                }
                int id = r.Next(0, walls.Count);
                int x = walls[id].x;
                int y = walls[id].y;
                //TODO: Improve this
                //Console.WriteLine(x + ", " + y +" count=" + walls.Count);
                if(x+1<m.width-1 && x-1>0 && m.mazeMap[x+1, y] == 1 && m.mazeMap[x-1, y] == 0){
                    m.mazeMap[x, y] = 0;
                    m.mazeMap[x+1, y] = 0;
                    addNeighbours(x+1, y);
                } else if(x+1<m.width-1 && x-1>0 && m.mazeMap[x+1, y] == 0 && m.mazeMap[x-1, y] == 1){
                    m.mazeMap[x, y] = 0;
                    m.mazeMap[x-1, y] = 0;
                    addNeighbours(x-1, y);
                } else if(y+1<m.height-1 && y-1>0 && m.mazeMap[x, y+1] == 1 && m.mazeMap[x, y-1] == 0){
                    m.mazeMap[x, y] = 0;
                    m.mazeMap[x, y+1] = 0;
                    addNeighbours(x, y+1);
                } else if(y+1<m.height-1 && y-1>0 && m.mazeMap[x, y+1] == 0 && m.mazeMap[x, y-1] == 1){
                    m.mazeMap[x, y] = 0;
                    m.mazeMap[x, y-1] = 0;
                    addNeighbours(x, y-1);
                }
                walls.RemoveAt(id);
            }
            m.drawMap();
            for (int i = 0; i < walls.Count; i++){
                m.drawSquare(walls[i].x, walls[i].y, 3);
            }
            return false;
        }

        bool randomWalk(int x, int y){
            List<Vector2i> c = new List<Vector2i>();
            if(x+1<m.width-1 && m.mazeMap[x+1,y] != 0 && visitedNeighbourNum(x+1, y) < 2) c.Add(new Vector2i(x+1, y));
            if(x-1>0 && m.mazeMap[x-1,y] != 0 && visitedNeighbourNum(x-1, y) < 2) c.Add(new Vector2i(x-1, y));
            if(y+1<m.height-1 && m.mazeMap[x,y+1] != 0 && visitedNeighbourNum(x, y+1) < 2) c.Add(new Vector2i(x, y+1));
            if(y-1>0 && m.mazeMap[x,y-1] != 0 && visitedNeighbourNum(x, y-1) < 2) c.Add(new Vector2i(x, y-1));
            if(c.Count<1){
                return false;
            }
            int id = r.Next(0, c.Count);
            this.x = c[id].x;
            this.y = c[id].y;
            return true;
        }

        bool huntSquare(){
            int startHunt = r.Next(1, m.width);
            for (int x = startHunt; x < m.width-1; x++){
                for (int y = 1; y < m.height-1; y++){
                    if(m.mazeMap[x, y] != 0 && visitedNeighbourNum(x, y) == 1){
                        this.x = x;
                        this.y = y;
                        //lastRow = x;
                        return true;
                    }
                }
            }
            for (int x = 1; x < startHunt; x++){
                for (int y = 1; y < m.height-1; y++){
                    if(m.mazeMap[x, y] != 0 && visitedNeighbourNum(x, y) == 1){
                        this.x = x;
                        this.y = y;
                        //lastRow = x;
                        return true;
                    }
                }
            }
            return false;
        }

        void placeExit(){
            m.mazeMap[m.endPoint.x, m.endPoint.y] = 0;
            int i = 1;
            while(m.mazeMap[m.endPoint.x, m.endPoint.y-i] != 0 && m.mazeMap[m.endPoint.x+1, m.endPoint.y-i+1] != 0 && m.mazeMap[m.endPoint.x-1, m.endPoint.y-i+1] != 0){
                m.mazeMap[m.endPoint.x, m.endPoint.y-i] = 0;
                i++;
            }
        }

        int visitedNeighbourNum(int x, int y){
            int num = 0;
            if(x+1<m.width-1 && m.mazeMap[x+1,y] == 0) num++;
            if(x-1>0 && m.mazeMap[x-1,y] == 0) num++;
            if(y+1<m.height-1 && m.mazeMap[x,y+1] == 0) num++;
            if(y-1>0 && m.mazeMap[x,y-1] == 0) num++;
            return num;
        }

        int unVisitedNeighbourNum(int x, int y){
            int num = 0;
            if(x+1<m.width-1 && m.mazeMap[x+1,y] != 0) num++;
            if(x-1>0 && m.mazeMap[x-1,y] != 0) num++;
            if(y+1<m.height-1 && m.mazeMap[x,y+1] != 0) num++;
            if(y-1>0 && m.mazeMap[x,y-1] != 0) num++;
            return num;
        }

        void addNeighbours(int x, int y){
            if(x+1<m.width-1 && m.mazeMap[x+1, y] != 0 && visitedNeighbourNum(x+1, y) < 2){ walls.Add(new Vector2i(x+1, y));}
            if(x-1>0 && m.mazeMap[x-1, y] != 0 && visitedNeighbourNum(x-1, y) < 2){ walls.Add(new Vector2i(x-1, y));}
            if(y+1<m.height-1 && m.mazeMap[x, y+1] != 0 && visitedNeighbourNum(x, y+1) < 2){ walls.Add(new Vector2i(x, y+1));}
            if(y-1>0 && m.mazeMap[x, y-1] != 0 && visitedNeighbourNum(x, y-1) < 2){ walls.Add(new Vector2i(x, y-1));}
        }
    }
}