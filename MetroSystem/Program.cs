using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MetroSystem
{
    public class Station
    {
        public string StationName;
        public Dictionary<Line, int> PlaceOfLine=new Dictionary<Line, int>();
        public bool isTrans;
        public int TransLineCount;
        public List<string> TransLine = new List<string>();
        public bool isBoundary=false;
        public List<string> EndStas = new List<string>();
        private int TransStaNo;

        public Station(string[] infos,Line l)
        {
            StationName = infos[0];
            int pointer = 1;
            if(infos.Length>1)
            {
                if (infos[1].Equals("IsBound"))
                {
                    isBoundary = true;
                    if (!EndStas.Contains(infos[2]))
                        EndStas.Add(infos[2]);
                    pointer = 3;
                }
            }
            if(infos.Length>pointer)
            {
                for (int i = pointer; i < infos.Length; i++)
                {
                    TransLine.Add(infos[i]);
                }
                TransLine.Add(l.LineName);
                TransLineCount = infos.Length-pointer+1;
                isTrans = true;
            }
            PlaceOfLine.Add(l, l.getStaCount()+1);
        }

        public void addLine(Line l)
        {
            if(!PlaceOfLine.ContainsKey(l))
                PlaceOfLine.Add(l, l.getStaCount() + 1);
        }
        public void setTransStaNo(int i)
        {
            if (isTrans)
                TransStaNo = i;
        }
        public int getTransStaNo()
        {
            return TransStaNo;
        }
        public override string ToString()
        {
            return StationName.ToString();
        }
        public void addEndSta(string[] infos)
        {
            if (infos[1].Equals("IsBound"))
            {
                isBoundary = true;
                if (!EndStas.Contains(infos[2]))
                    EndStas.Add(infos[2]);
            }
        }
    }

    public class Line
    {
        public string LineName;
        public List<Station> Stations=new List<Station>();
        public List<Tuple<string, Station>> TransStations = new List<Tuple<string, Station>>();
        public bool isRoundLine;
        private int LineTransSta=0;

        public Line(string name)
        {
            LineName = name;
        }

        public int getStaCount()
        {
            return Stations.Count;
        }

        public void addSta(Station s)
        {
            Stations.Add(s);
            if(s.isTrans)
            {
                foreach (string lName in s.TransLine)
                {
                    if(!lName.Equals(LineName))
                        TransStations.Add(Tuple.Create(lName, s));
                }
                LineTransSta++;
                    
            }
        }
        public int TransStaCount()
        {
            return LineTransSta;
        }

    }
    public class PathSection
    {
        private LinkedList<Station> list = new LinkedList<Station>();
        public string LineName;
        public bool isTrans=true;
        public Station from;
        public Station to;

        public PathSection(string s)
        {
            LineName = s;
               
        }
        public override string ToString()
        {
            string s=null;
            if(isTrans)
                s +="换乘"+LineName;
            foreach(Station sta in list)
            {
                s +="\n"+sta;
            }
            return s;
        }
        public void addSta(Station s)
        {
            if (list.Count == 0)
                from = s;
            list.AddLast(s);
            to = s;
            
        }
        public int GetLen()
        {
            return list.Count();
        }
    }
    class Program
    {
        public Dictionary<string, Station> StaCollection=new Dictionary<string, Station>();
        Dictionary<string, Line> LineCollection=new Dictionary<string, Line>();
        Dictionary<string, int> NameToNo = new Dictionary<string, int>();
        public Dictionary<int, string> NoToName = new Dictionary<int, string>();
        Dictionary<string, Station> TransStaCollection = new Dictionary<string, Station>();
        static int MAXN=60;
        private static int[,] graph = new int[MAXN,MAXN];//The Adjacency matrix of the graph  
        private static int[,] graph1 = new int[MAXN, MAXN];//The Adjacency matrix of the graph  

        private int _minLine;
        private int _minTransCount;
        private List<List<PathSection>> _shortestLines=new List<List<PathSection>>();
        private LinkedList<PathSection> _shortestWays=new LinkedList<PathSection>();

        private static int TransStaCount = 0;
        public void ReadData()
        {
            FileStream fs = new FileStream("beijing-subway.txt", FileMode.Open);
            StreamReader sr = new StreamReader(fs, Encoding.Default);
            //StreamReader sr = File.OpenText("beijing-subway.txt");
            string readIn = sr.ReadLine();
            while (readIn != null)
            {
                if (readIn.Equals("BEGIN"))
                {
                    string isRound = sr.ReadLine();
                    string name = sr.ReadLine();
                    Line MetroLine = new Line(name);
                    if (isRound.Equals("环线"))
                    {
                        MetroLine.isRoundLine = true;
                    }
                    if(!LineCollection.ContainsKey(name))
                        LineCollection.Add(name, MetroLine);
                    readIn = sr.ReadLine();
                    while (readIn != "END" && readIn != null)
                    {
                        string[] infos = readIn.Split(' ');
                        if (!StaCollection.ContainsKey(infos[0]))
                            StaCollection.Add(infos[0], new Station(infos,MetroLine));
                        else
                        {
                            StaCollection[infos[0]].addLine(MetroLine);
                            if(infos.Length>1)
                            {
                                if (infos[1].Equals("IsBound"))
                                    StaCollection[infos[0]].addEndSta(infos);
                            } 
                        }
                        if(StaCollection[infos[0]].isTrans&&(!TransStaCollection.ContainsKey(infos[0])))
                        {
                            TransStaCollection.Add(infos[0], StaCollection[infos[0]]);
                            TransStaCount++;
                            NameToNo.Add(infos[0], TransStaCount);
                            NoToName.Add(TransStaCount, infos[0]);
                            StaCollection[infos[0]].setTransStaNo(TransStaCount);
                        }
                        readIn = sr.ReadLine();
                        MetroLine.addSta(StaCollection[infos[0]]);
                    }
                    
                }
                else
                    readIn = sr.ReadLine();
            }
        }
        public void BuildGragph(string old1,string old2,string new1)
        {
            int BigNum = 99999;
            int pointer = 0;
            for (int i=0;i<MAXN; i++)
            {
                for(int j=0;j<MAXN;j++)
                {
                    if (i == j)
                    {
                        graph[i, j] = 0;
                        graph1[i, j] = 0;
                    }
                    else
                    {
                        graph[i, j] = BigNum;
                        graph1[i, j] = BigNum;
                    }
                }
            }
            if (!new1.Equals(""))
                pointer = 1;
            for (int i = 1; i <= TransStaCount-pointer; i++)
            {
                List<Station> temp = GetLinkedStations(TransStaCollection[NoToName[i]]);
                foreach(Station s in temp)
                {
                    if(graph[i,NameToNo[s.StationName]]==0|| graph[i,NameToNo[s.StationName]]==BigNum) //优化时间
                        graph[i,NameToNo[s.StationName]] = SectionLen(TransStaCollection[NoToName[i]], s);
                }
                temp.Clear();
                temp = GetShortestLinkedStations(TransStaCollection[NoToName[i]]);
                foreach (Station s in temp)
                {
                    if (graph1[i, NameToNo[s.StationName]] == 0 || graph1[i, NameToNo[s.StationName]] == BigNum) //优化时间
                        graph1[i, NameToNo[s.StationName]] = SectionLen(TransStaCollection[NoToName[i]], s);
                }
            }
            if (!new1.Equals(""))
            {
                graph1[NameToNo[old1], NameToNo[old2]] = BigNum;
                graph1[NameToNo[old2], NameToNo[old1]] = BigNum;
                graph1[NameToNo[old1], NameToNo[new1]] = SectionLen(StaCollection[old1], StaCollection[new1]);
                graph1[NameToNo[old2], NameToNo[new1]] = SectionLen(StaCollection[old2], StaCollection[new1]);
                graph1[NameToNo[new1], NameToNo[old1]] = SectionLen(StaCollection[old1], StaCollection[new1]);
                graph1[NameToNo[new1], NameToNo[old2]] = SectionLen(StaCollection[old2], StaCollection[new1]);
            }
        }
        public Tuple<string,int> DjistraPath(string f,string t)
        {
            if (!StaCollection.ContainsKey(f))
                return Tuple.Create(f + " is not contain", 0);
            if (!StaCollection.ContainsKey(t))
                return Tuple.Create(t + " is not contain", 0);
            if (f.Equals(t))
                return Tuple.Create("起点站与终点站相同", 0);
            int len = 0;
            int extralen = 0;
            Station from = StaCollection[f];
            Station to = StaCollection[t];
            string extrastring="";
            if (from.StationName.Equals("2号航站楼"))
            {
                if(to.StationName.Equals("3号航站楼"))
                {
                    string temp = "\n三元桥\n3号航站楼";
                    return Tuple.Create(temp, 2);
                }
                else
                    from = StaCollection["3号航站楼"];
            }
            else if (from.StationName.Equals("3号航站楼"))
            {
                if (to.StationName.Equals("2号航站楼"))
                {
                    string temp = "\n2号航站楼";
                    return Tuple.Create(temp, 1);
                }
                else
                    extrastring = "\n2号航站楼";
                extralen= 1;
            }
            LinkedList<PathSection> path = new LinkedList<PathSection>();
            if (isSameLine(from, to) != null)
            {
                PathSection newsection = MakePathSection(from, to);
                len = newsection.GetLen();
                path.AddFirst(newsection);
                return Tuple.Create(extrastring+HandlePath(path), len+extralen);
            }

            int[] parents1 = new int[TransStaCount + 1];//The shortest distence between 0 and N  
            int[] parents2 = new int[TransStaCount + 1];
            int[] parents3 = new int[TransStaCount + 1];
            int[] parents4 = new int[TransStaCount + 1];
            int FromNo1 = -1;
            int ToNo1 = -1;
            int FromNo2 = -1;
            int ToNo2 = -1;
            int BigNum = 99999;
            int len1=BigNum, len2=BigNum, len3=BigNum, len4=BigNum;
            
            if (from.isTrans)
            {
                FromNo1 = NameToNo[from.StationName];
                if(to.isTrans)
                {
                    ToNo1 = NameToNo[to.StationName];
                    len = DjistraLen(FromNo1, ToNo1, parents1);
                    path = BuildPath(parents1, FromNo1, ToNo1);
                    return Tuple.Create(extrastring + HandlePath(path), len + extralen);
                }
                else
                {
                    Line temp = to.PlaceOfLine.First().Key;
                    for (int i= to.PlaceOfLine.First().Value-1;i>0;i--)
                    {
                        if (temp.Stations.ElementAt(i - 1).isTrans)
                        {
                            ToNo1 = NameToNo[temp.Stations.ElementAt(i - 1).StationName];
                            len1 = DjistraLen(FromNo1, ToNo1, parents1);
                            len1 += SectionLen(temp.Stations.ElementAt(i - 1), to);
                            break;
                        } 
                    }
                    for (int i = to.PlaceOfLine.First().Value+1; i <=to.PlaceOfLine.First().Key.Stations.Count; i++)
                    {
                        if (temp.Stations.ElementAt(i - 1).isTrans)
                        {
                            ToNo2 = NameToNo[temp.Stations.ElementAt(i - 1).StationName];
                            len2 = DjistraLen(FromNo1, ToNo2, parents2);
                            len2 += SectionLen(temp.Stations.ElementAt(i - 1), to);
                            break;
                        }
                    }
                    if(len1<len2)
                    {
                        len = len1;
                        path = BuildPath(parents1, FromNo1, ToNo1);
                        path.AddLast(MakePathSection(StaCollection[NoToName[ToNo1]], to));
                        return Tuple.Create(extrastring + HandlePath(path), len + extralen);
                    }
                    else
                    {
                        len = len2;
                        path = BuildPath(parents2, FromNo1, ToNo2);
                        path.AddLast(MakePathSection(StaCollection[NoToName[ToNo2]], to));
                        return Tuple.Create(extrastring + HandlePath(path), len + extralen);
                    }
                }
            }
            else
            {
                Line temp = from.PlaceOfLine.First().Key;
                for (int i = from.PlaceOfLine.First().Value - 1; i > 0; i--)
                {
                    if (temp.Stations.ElementAt(i - 1).isTrans)
                    {
                        FromNo1 = NameToNo[temp.Stations.ElementAt(i - 1).StationName];
                        break;
                    }
                        
                }
                for (int i = from.PlaceOfLine.First().Value + 1; i <= from.PlaceOfLine.First().Key.Stations.Count; i++)
                {
                    if (temp.Stations.ElementAt(i - 1).isTrans)
                    {
                        FromNo2 = NameToNo[temp.Stations.ElementAt(i - 1).StationName];
                        break;
                    }
                }

                if (to.isTrans)
                {
                    ToNo1 = NameToNo[to.StationName];
                    if(FromNo1!=-1)
                    {
                        len1 = DjistraLen(FromNo1, ToNo1, parents1);
                        len1 += SectionLen(from, StaCollection[NoToName[FromNo1]]);
                    }
                    if(FromNo2!=-1)
                    {
                        len2 = DjistraLen(FromNo2, ToNo1, parents2);
                        len2 += SectionLen(from, StaCollection[NoToName[FromNo2]]);
                    }
                    if (len1 < len2)
                    {
                        len = len1;
                        path = BuildPath(parents1, FromNo1, ToNo1);
                        path.AddFirst(MakePathSection(from,StaCollection[NoToName[FromNo1]]));
                        return Tuple.Create(extrastring + HandlePath(path), len + extralen);
                    }
                    else
                    {
                        len = len2;
                        path = BuildPath(parents2, FromNo2, ToNo1);
                        path.AddFirst(MakePathSection(from, StaCollection[NoToName[FromNo2]]));
                        return Tuple.Create(extrastring + HandlePath(path), len + extralen);
                    }
                }
                else
                {
                    temp = to.PlaceOfLine.First().Key;
                    for (int i = to.PlaceOfLine.First().Value - 1; i > 0; i--)
                    {
                        if (temp.Stations.ElementAt(i - 1).isTrans)
                        {
                            ToNo1 = NameToNo[temp.Stations.ElementAt(i - 1).StationName];
                            break;
                        }
                    }
                    for (int i = to.PlaceOfLine.First().Value + 1; i <= to.PlaceOfLine.First().Key.Stations.Count; i++)
                    {
                        if (temp.Stations.ElementAt(i - 1).isTrans)
                        {
                            ToNo2 = NameToNo[temp.Stations.ElementAt(i - 1).StationName];
                            break;
                        }
                    }
                    if(FromNo1!=-1&&ToNo1!=-1)
                    {
                        len1 = DjistraLen(FromNo1, ToNo1, parents1);
                        len1 += SectionLen(from, StaCollection[NoToName[FromNo1]]);
                        len1 += SectionLen(StaCollection[NoToName[ToNo1]], to);
                    }
                    if (FromNo2 != -1 && ToNo1 != -1)
                    {
                        len2 = DjistraLen(FromNo2, ToNo1, parents2);
                        len2 += SectionLen(from, StaCollection[NoToName[FromNo2]]);
                        len2 += SectionLen(StaCollection[NoToName[ToNo1]], to);
                    }
                    if (FromNo1 != -1 && ToNo2 != -1)
                    {
                        len3 = DjistraLen(FromNo1, ToNo2, parents3);
                        len3 += SectionLen(from, StaCollection[NoToName[FromNo1]]);
                        len3 += SectionLen(StaCollection[NoToName[ToNo2]], to);
                    }
                    if (FromNo2 != -1 && ToNo2 != -1)
                    {
                        len4 = DjistraLen(FromNo2, ToNo2, parents4);
                        len4 += SectionLen(from, StaCollection[NoToName[FromNo2]]);
                        len4 += SectionLen(StaCollection[NoToName[ToNo2]], to);
                    }
                    if (len1 <= len2 && len1 <= len3 && len1 <= len4)
                    {
                        len = len1;
                        path = BuildPath(parents1, FromNo1, ToNo1);
                        path.AddFirst(MakePathSection(from, StaCollection[NoToName[FromNo1]]));
                        path.AddLast(MakePathSection(StaCollection[NoToName[ToNo1]], to));
                        return Tuple.Create(extrastring + HandlePath(path), len + extralen);
                    }
                    else if (len2 <= len1 && len2 <= len3 && len2 <= len4)
                    {
                        len = len2;
                        path = BuildPath(parents2, FromNo2, ToNo1);
                        path.AddFirst(MakePathSection(from, StaCollection[NoToName[FromNo2]]));
                        path.AddLast(MakePathSection(StaCollection[NoToName[ToNo1]], to));
                        return Tuple.Create(extrastring + HandlePath(path), len + extralen);
                    }
                    else if (len3 <= len2 && len3 <= len1 && len3 <= len4)
                    {
                        len = len3;
                        path = BuildPath(parents3, FromNo1, ToNo2);
                        path.AddFirst(MakePathSection(from, StaCollection[NoToName[FromNo1]]));
                        path.AddLast(MakePathSection(StaCollection[NoToName[ToNo2]], to));
                        return Tuple.Create(extrastring + HandlePath(path), len + extralen);
                    }
                    else
                    {
                        len = len4;
                        path = BuildPath(parents4, FromNo2, ToNo2);
                        path.AddFirst(MakePathSection(from, StaCollection[NoToName[FromNo2]]));
                        path.AddLast(MakePathSection(StaCollection[NoToName[ToNo2]], to));
                        return Tuple.Create(extrastring + HandlePath(path), len + extralen);
                    }
                }
                return null;
            }
        }
        public int DjistraLen(int fromno, int tono, int[] parents)
        {
            if (fromno == tono)
                return 0;
            int[] dist = new int[TransStaCount + 1];//The shortest distence between 0 and N  
            bool[] vis = new bool[TransStaCount + 1];//Sign the point which is visited
            int BigNum = 99999;
            int n = TransStaCount;
            for (int i = 1; i <= n; i++)
            {
                dist[i] = graph[fromno,i];//Set the dist[]  
                if (dist[i] < BigNum)
                    parents[i] = fromno;
            }
            vis[fromno] = true; int min, v = fromno;
            for (int i = 1; i <=n - 1; i++)
            {//Check n-1 times  
                min = BigNum;
                for (int j = 1; j <= n; j++)
                {//Find shortest  
                    if (vis[j] != true && dist[j] < min)
                    {//If the point is not visited and the distence between 0 and j is smallest mark the point j  
                        min = dist[j];
                        v = j;
                    }
                    
                }
                vis[v] = true;        //Sign the point v to be visited   
                if (v == tono)
                    break;
                for (int j = 1; j <=n; j++)
                {//Refresh the dist[]  
                    if (vis[j] != true && dist[j] > dist[v] + graph[v,j])
                    {//when distence is shorter when pass the point v refresh the dist  
                        dist[j] = dist[v] + graph[v,j];
                        parents[j] = v;
                    }
                }
            }
            return dist[tono];
        }
        public Tuple<string, int> BFSPath(string f, string t)
        {
            //验证站点存在
            if (!StaCollection.ContainsKey(f))
                return Tuple.Create(f + " is not contain", 0);
            if (!StaCollection.ContainsKey(t))
                return Tuple.Create(t + " is not contain", 0);
            if (f.Equals(t))
                return Tuple.Create("起点站与终点站相同", 0);
            int extralen = 0;
            Station from = StaCollection[f];
            Station to = StaCollection[t];
            string extrastring = "";
            if (from.StationName.Equals("2号航站楼"))
            {
                if (to.StationName.Equals("3号航站楼"))
                {
                    string temp = "\n三元桥\n3号航站楼";
                    return Tuple.Create(temp, 2);
                }
                else
                    from = StaCollection["3号航站楼"];
            }
            else if (from.StationName.Equals("3号航站楼"))
            {
                if (to.StationName.Equals("2号航站楼"))
                {
                    string temp = "\n2号航站楼";
                    return Tuple.Create(temp, 1);
                }
                else
                    extrastring = "\n2号航站楼";
                extralen = 1;
            }
            List<PathSection> stationList;
            List<Line> lineHis;

            //重调两个最值
            _minLine = _minTransCount = int.MaxValue;

            //遍历这个起点站所在的线路，然后分别从这些线路出发去寻找目的站点
            foreach (KeyValuePair<Line, int> line in from.PlaceOfLine)
            {
                stationList = new List<PathSection>();
                lineHis = new List<Line>() { line.Key};
                BFSPathRecursion(0, from, line.Key, to, stationList, lineHis);
            }
            //去除站点较多的线路
            ClearLongerWays();
            //生成线路的字符串
            string result = HandlePath(_shortestWays);

            //清空整个查找过程中线路数据
            _shortestLines.Clear();
            _shortestWays.Clear();

            return Tuple.Create(extrastring+result,extralen+_minLine);
        }
        public void BFSPathRecursion(int transLv, Station curStation, Line curLine,Station endStation, List<PathSection> stationList,List<Line> lineHis)
        {
            //如果当前换乘的次数比换乘次数最小值
            //就不用再找了，找出来的线路肯定更长
            if (transLv > _minTransCount) return;
            //判定是否已经到达目标站的线路了，若是表明一直查找成功了
            if (isSameLine(curStation, endStation)!=null)
            {
                PathSection s2s =MakePathSection(curStation,endStation);
                stationList.Add(s2s);
                //若当前换乘次数比记录值要小，清空之前的线路段
                if (_minTransCount > transLv)
                    _shortestLines.Clear();

                _shortestLines.Add(stationList.ToArray().ToList());
                stationList.Remove(s2s);
                _minTransCount = transLv;
                return;
            }
            List<Tuple<string, Station>> transform = curLine.TransStations;
            if (curLine.TransStaCount() == 1)
            {
                lineHis.Remove(curLine);
                return;
            }
            //遍历一下当前线路的换乘站，从而递归找出到目标站的线路
            foreach (Tuple<string, Station> item in transform)
            {
                //如果这条线路已经到过的，进入下次循环
                if (lineHis.Contains(LineCollection[item.Item1]))
                    continue;
                lineHis.Add(LineCollection[item.Item1]);
                PathSection s2s = MakePathSection(curStation, item.Item2);
                stationList.Add(s2s);
                //递归调用
                BFSPathRecursion(transLv + 1, item.Item2, LineCollection[item.Item1], endStation, stationList, lineHis);
                //清除集合里的值，以这种方式减少内存使用量，提高效率
                lineHis.Remove(LineCollection[item.Item1]);
                stationList.Remove(s2s);
            }
        }
        public void ClearLongerWays()
        {
            _shortestWays.Clear();
            int curCount = 0;
            LinkedList<PathSection> way = null;
            foreach (List<PathSection> innerList in _shortestLines)
            {
                curCount = 0;
                way = new LinkedList<PathSection>();
                foreach (PathSection item in innerList)
                {
                    curCount += item.GetLen();
                    if (curCount > _minLine)
                        break;
                    way.AddLast(item);
                }
                //if (curCount == _minLine)
                //    _shortestWays.Add(way);
                if (curCount<_minLine)
                {
                    _shortestWays.Clear();
                    _shortestWays=way;
                    _minLine = curCount;
                }
            }
        }
        public LinkedList<PathSection> BuildPath(int[] parents,int from,int to)
        {
            LinkedList<PathSection> list = new LinkedList<PathSection>();
            if (from == to)
                return list;
            Station start, end;
            int s, e;
            e = to;
            s = parents[e];
            while(e!=from)
            {
                start = StaCollection[NoToName[s]];
                end = StaCollection[NoToName[e]];
                PathSection p = MakePathSection(start, end);
                list.AddFirst(p);
                e = s;
                s = parents[e];
            }
            return list;
        }
        public string HandlePath(LinkedList<PathSection> list)
        {
            if (list.Count == 0)
                return null;
            PathSection cur = list.First();
            list.RemoveFirst();
            string formerline = cur.LineName;
            cur.isTrans = false;
            string path = cur.ToString();
            if (list.Count == 0)
            {
                
                return path;
            }
            cur = list.First();
            while(cur!=null)
            {
                if (cur.LineName != formerline)
                    cur.isTrans = true;
                else
                    cur.isTrans = false;
                formerline = cur.LineName;
                path += cur.ToString();
                list.RemoveFirst();
                if (list.Count == 0)
                    break;
                cur = list.First();
            }
            return path;
        
        }
        public PathSection MakePathSection(Station from,Station to)
        {
            Line l = isSameLine(from, to);
            PathSection p = new PathSection(l.LineName);
            int start = from.PlaceOfLine[l];
            int end = to.PlaceOfLine[l];
            if(l.isRoundLine&&( SectionLen(from, to) < Math.Abs(start - end)))
            {
                if (start < end)
                {
                    for (int i = start-2; i >=0; i--)
                        p.addSta(l.Stations.ElementAt(i));
                    for (int i = l.Stations.Count-1; i >= end-1; i--)
                        p.addSta(l.Stations.ElementAt(i));
                }
                else
                {
                    for (int i = start ; i <= l.Stations.Count - 1; i++)
                        p.addSta(l.Stations.ElementAt(i));
                    for (int i = 0; i <= end - 1; i++)
                        p.addSta(l.Stations.ElementAt(i));
                }
            }
            else
            {
                if(start < end)
                {
                    for (int i = start; i <=end-1; i++)
                        p.addSta(l.Stations.ElementAt(i));
                }
                else
                {
                    for (int i = start - 2; i >= end-1; i--)
                        p.addSta(l.Stations.ElementAt(i));
                }
            }
            return p;
        }
        public int SectionLen(Station s1,Station s2)
        {
            Line temp = isSameLine(s1, s2);
            int len=999999;
            if (temp!=null)
            {
                if(temp.isRoundLine)
                {
                    int clockwise = Math.Abs(s1.PlaceOfLine[temp] - s2.PlaceOfLine[temp]);
                    int anticlockwise = temp.Stations.Count - Math.Abs(s1.PlaceOfLine[temp] - s2.PlaceOfLine[temp]);
                    if (clockwise < anticlockwise)
                        len = clockwise;
                    else
                        len = anticlockwise;
                }
                else
                    len = Math.Abs(s1.PlaceOfLine[temp] - s2.PlaceOfLine[temp]);
            }
            return len;
        }
        public Line isSameLine(Station s1,Station s2)
        {
            foreach(Line l in s1.PlaceOfLine.Keys)
            {
                if (l.Stations.Contains(s2))
                    return l;
            }
            return null;
        }
        public List<Station> GetLinkedStations(Station s)
        {
            List<Station> linksta = new List<Station>();
            foreach (Line l in s.PlaceOfLine.Keys)
            {
                foreach(Tuple<string,Station> t  in l.TransStations)
                {
                    if(!(linksta.Contains(t.Item2)||t.Item2.StationName.Equals(s.StationName)))
                    {
                        linksta.Add(t.Item2);
                    }
                }
            }
            return linksta;
        }
        public List<Station> GetShortestLinkedStations(Station s)
        {
            List<Station> linksta = new List<Station>();
            foreach (Line l in s.PlaceOfLine.Keys)
            {
                Station now = s;
                bool flag = false;
                int place = l.Stations.IndexOf(s);
                for(int i=place;i>=0;i--)
                {
                    now = l.Stations.ElementAt(i);
                    if (now.isTrans && (!(linksta.Contains(now) || now.StationName.Equals(s.StationName))))
                    {
                        linksta.Add(now);
                        flag = true;
                        break;
                    }
                }
                if(flag==false&&l.isRoundLine)
                {
                    for (int i = l.Stations.Count-1; i > place; i--)
                    {
                        now = l.Stations.ElementAt(i);
                        if (now.isTrans && (!(linksta.Contains(now) || now.StationName.Equals(s.StationName))))
                        {
                            linksta.Add(now);
                            break;
                        }
                    }
                }
                flag = false;
                for (int i = place; i <l.Stations.Count; i++)
                {
                    now = l.Stations.ElementAt(i);
                    if (now.isTrans && (!(linksta.Contains(now) || now.StationName.Equals(s.StationName))))
                    {
                        linksta.Add(now);
                        flag = true;
                        break;
                    }
                }
                if (flag ==false && l.isRoundLine)
                {
                    for (int i = 0; i < place; i++)
                    {
                        now = l.Stations.ElementAt(i);
                        if (now.isTrans && (!(linksta.Contains(now) || now.StationName.Equals(s.StationName))))
                        {
                            linksta.Add(now);
                            break;
                        }
                    }
                }
            }
            return linksta;
            
        }
        static void Main(string[] args)
        {
            Program metrosys = new Program();
            metrosys.ReadData();
            metrosys.BuildGragph("","","");

            /*ChinPost cp = new ChinPost(metrosys);
            cp.Initial(graph1, TransStaCount, 1);
            cp.OddDeal();
            cp.Fleury(1);
            Console.ReadLine();
            Station f = metrosys.StaCollection[args[1]];
            Station t = metrosys.StaCollection[args[2]];
            
            string f1="南邵";
            string f2 = "角门西";
            string f3 = "亦庄火车站";
            string f4 = "木樨地";
            string f5 = "灯市口";
            string f6 = "达官营";
            string f7 = "大瓦窑";
            string f8 = "安河桥北";
            string f9 = "善各庄";
            string f10 = "车公庄";
            Tuple<string, int> p = metrosys.DjistraPath(f1, f2);
            Tuple<string, int> p1 = metrosys.DjistraPath(f2, f3);
            Tuple<string, int> p2 = metrosys.DjistraPath(f4, f1);
            Tuple<string, int> p3 = metrosys.DjistraPath(f3, f5);
            Tuple<string, int> p4 = metrosys.DjistraPath(f5, f3);
            Tuple<string, int> p5 = metrosys.DjistraPath(f1, f7);
            Tuple<string, int> p6 = metrosys.DjistraPath(f3, f10);
            Tuple<string, int> p7 = metrosys.BFSPath(f1, f2);
            Tuple<string, int> p8 = metrosys.BFSPath(f2, f3);
            Tuple<string, int> p9 = metrosys.BFSPath(f4, f1);
            Tuple<string, int> p10 = metrosys.BFSPath(f3, f5);
            Tuple<string, int> p11 = metrosys.BFSPath(f5, f3);
            Tuple<string, int> p12 = metrosys.BFSPath(f1, f7);
            Tuple<string, int> p13 = metrosys.BFSPath(f3, f10);
            System.Console.Write(p.Item2);
            System.Console.Write(p.Item1);
            //System.Console.Write(q.Item2);
            //System.Console.Write(q.Item1);
            Console.ReadLine();
            */
            if (args.Length == 1 && args[0].Equals("exit"))
            {
                System.Console.WriteLine("程序结束");
                return;
            }
            else if(args.Length == 2 && args[0].Equals("-p"))
            {
                if(metrosys.LineCollection.ContainsKey(args[1]))
                {
                    System.Console.WriteLine(metrosys.LineCollection[args[1]].LineName);
                    foreach (Station s in metrosys.LineCollection[args[1]].Stations)
                    {
                        System.Console.WriteLine(s.StationName);
                    }
                }
                else
                {
                    System.Console.WriteLine("不存在这条线路，请重试");
                }
                
            }
            else if (args.Length != 2 && args[0].Equals("-a"))
            {
                System.Console.WriteLine("使用-a功能请只设置一个起点站，请重试");
            }
            else if (args.Length != 3&&!args[0].Equals("-a"))
            {
                System.Console.WriteLine("不正确的格式，请重试");
            }
            else
            {
                string f = args[1];
                string t = "";
                if (args.Length>2)
                    t = args[2];
                switch (args[0])
                {
                    case "-c":
                        Tuple<string, int> p1 = metrosys.BFSPath(f, t);
                        System.Console.Write(p1.Item2);
                        System.Console.WriteLine(p1.Item1);
                        break;
                    case "-b":
                        Tuple<string,int> p2 = metrosys.DjistraPath(f, t);
                        System.Console.Write(p2.Item2);
                        System.Console.WriteLine(p2.Item1);
                        break;
                    case "-a":
                        int startstano = -1;
                        int addcount = 0;
                        string string_in = "";
                        string string_out = "";
                        if (!metrosys.StaCollection.ContainsKey(f))
                        {
                            System.Console.WriteLine("起点车站不存在，请重试");
                            break;
                        }
                        if(metrosys.StaCollection[f].isTrans)
                        {
                            startstano = metrosys.NameToNo[f];
                        }
                        else
                        {
                            Station s = metrosys.StaCollection[f];
                            Line l = s.PlaceOfLine.First().Key;
                            int start_place= s.PlaceOfLine.First().Value;
                            bool flag_low = false;
                            bool flag_high = false;
                            int low_place = -1;
                            int high_place = -1;
                            if(start_place!=0)
                            {
                                for (int i = start_place - 1; i >= 0; i--)
                                {
                                    if (l.Stations.ElementAt(i).isTrans)
                                    {
                                        flag_low = true;
                                        low_place = i;
                                        break;
                                    }
                                }
                            }
                            if(start_place!=l.Stations.Count-1)
                            {
                                for (int i = start_place + 1; i <= l.Stations.Count-1; i++)
                                {
                                    if (l.Stations.ElementAt(i).isTrans)
                                    {
                                        flag_high = true;
                                        high_place = i;
                                        break;
                                    }
                                }
                            }
                            if(flag_high==true&&flag_low==true)
                            {
                                metrosys.TransStaCollection.Add(f,s);
                                TransStaCount++;
                                metrosys.NameToNo.Add(f, TransStaCount);
                                metrosys.NoToName.Add(TransStaCount, f);
                                metrosys.StaCollection[f].setTransStaNo(TransStaCount);
                                string new1 = f;
                                string old1 = l.Stations.ElementAt(high_place).StationName;
                                string old2= l.Stations.ElementAt(low_place).StationName;
                                metrosys.BuildGragph(old1,old2,new1);
                                startstano = TransStaCount;
                            }
                            else if(flag_high==true)
                            {
                                string_in = string_in + f + "-" + l.Stations.First().StationName + "\n" + l.Stations.First().StationName + "-" + l.Stations.ElementAt(high_place).StationName + "\n";
                                string_out = string_out + l.Stations.ElementAt(high_place).StationName + "-" + f+"\n";
                                addcount +=metrosys.SectionLen(l.Stations.ElementAt(high_place), l.Stations.First()) *2;
                                startstano = metrosys.NameToNo[l.Stations.ElementAt(high_place).StationName];
                                l.Stations.ElementAt(high_place).EndStas.Remove(l.Stations.First().StationName);
                            }
                            else if(flag_low==true)
                            {
                                string_in = string_in + f + "-" + l.Stations.Last().StationName + "\n" + l.Stations.Last().StationName + "-" + l.Stations.ElementAt(low_place).StationName + "\n";
                                string_out = string_out + l.Stations.ElementAt(low_place).StationName + "-" + f+"\n";
                                addcount += metrosys.SectionLen(l.Stations.ElementAt(low_place), l.Stations.Last()) * 2;
                                startstano = metrosys.NameToNo[l.Stations.ElementAt(low_place).StationName];
                                l.Stations.ElementAt(low_place).EndStas.Remove(l.Stations.Last().StationName);
                            }
                            else
                            {
                                System.Console.WriteLine("这是一个奇怪的站");
                                break;
                            }
                        }
                        ChinPost cp = new ChinPost(metrosys);
                        cp.Initial(graph1, TransStaCount,startstano);
                        cp.OddDeal();
                        cp.Fleury(startstano,string_in,string_out,addcount);
                        break;
                    default:
                        System.Console.WriteLine("错误的参数，请重试");
                        break;
                }
            }
            
        }
    }
}
