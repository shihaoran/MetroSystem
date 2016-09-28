using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroSystem
{
    class ChinPost
    {
        static int MAX_NODE = 60; // 最大结点数
        int COST_NO_LINK = int.MaxValue; // 定义结点之间没有连接的花销为INT_MAX吗

        int[,] Graph = new int[MAX_NODE,MAX_NODE];
        int[,] Cost = new int[MAX_NODE, MAX_NODE];
        int V_dingdianshu, E_bianshu, Start_Point; // 顶点数和边数，以及开始的起点（以0开始）

        int []Odd_Grouping=new int[MAX_NODE]; // 为0表示不为奇，为1表示为奇，从2开始表示配对分组情况，如同为2的两个为一组，同为3的两个为一组，……
        int []Bak_Odd_Grouping=new int[MAX_NODE]; // 最好情况下分组策略的备份，因为可能还有其他情况更好，如果有，就更新此备份。
        int SHORTEST_PATH_WEIGHT = int.MaxValue; // 如果存在奇度数点，这里是记录的添加最短路径的最小值，是所有点两两分组的最短路径之和的最小值。此值对应Bak_Odd_Grouping所描述的分组情况。

        int []Dist=new int[MAX_NODE]; // Dijstra算法中，求从v0到v1最短路径结果，里面包含v0到最短路径上各点的最短权值
        int [,]ShortCache=new int[MAX_NODE,MAX_NODE]; // Dijstra算法中，求点v0到v1的最短路径记录值，当第一次求时，把结果存到本数组中，下次如果还在相同调用，则直接返回本数组中相应值。
        Dictionary<int, string> NoToName;
        bool find = false;
        Program metrosys;
        // 数据输入，会用到Graph和Cost
        public ChinPost(Program p)
        {
            NoToName = p.NoToName;
            metrosys = p;
        }

        public void Initial(int[,] graph,int dingdian,int start)
        {
            int i, j;
            V_dingdianshu = dingdian;
            Start_Point = start;
            for (i = 0; i < MAX_NODE; i++)
            {
                for (j = 0; j < MAX_NODE; j++)
                {
                    if (graph[i, j] != 0 && graph[i, j] != 99999)
                        Graph[i, j] = 1;
                    else
                        Graph[i, j] = 0;
                    ShortCache[i,j] = 0;
                    Cost[i,j] = graph[i,j];
                }
            }
        }

        // 对图G从v0点开始到v1点算到必要各点的最短距离，结果保存在dist上面。因此不保证dist上的所有数据都是正确的（保证dist[v]是从v0到v的最短距离）
        // 第三个参数指定是否使用Cache值，如果为真，则一般Dist的值不与当前调用相对应，反之则保证dist[v]是从v0到v的最短距离
        // 返回dist[v1]，即v0到v1的最短距离值
        public int Dijstra(int v0, int v1, bool useCache)
        {
            if (useCache && ShortCache[v0,v1] != 0) // 之前计算过了，直接返回值
            {
                return ShortCache[v0,v1];
            }

            int i, s, w, min;
            int minIndex=-1;
            bool []Final=new bool[MAX_NODE];

            // 初始化最短路径长度数据，所有数据都不是最终数据 
            for (s = 1; s <=V_dingdianshu; s++)
            {
                Final[s] = false;
                Dist[s] = COST_NO_LINK; // 初始最大距离
            }
            // 首先选v0到v0的距离一定最短，最终数据 
            Final[v0] = true;
            Dist[v0] = 0;
            s = v0; // 0 预先选中v0点

            for (i = 1; i <=V_dingdianshu; i++)
            {
                // 1 更新该点到其他未选中点的最短路径
                for (w = 1; w <=V_dingdianshu; w++)
                {
                    if (!Final[w] // w点未选中
                        && Cost[s,w] < COST_NO_LINK // 更新点应该与选中点s相连
                        && Dist[w] > Dist[s] + Cost[s,w]) // 通过点s会有更短的路径
                    {
                        if (Dist[s] + Cost[s,w] <= 0)
                        {
                            System.Console.WriteLine("求最短路径数据溢出。");
                            System.Environment.Exit(-1);
                        }
                        Dist[w] = Dist[s] + Cost[s,w];
                    }
                }
                // 1.5 如果在中间过程找到了目标点v1，则不再继续计算了
                if (s == v1)
                {
                    ShortCache[v0,v1] = Dist[s];
                    ShortCache[v1,v0] = Dist[s];
                    return Dist[s];
                }
                // 2 选中相应点
                min = COST_NO_LINK;
                for (w = 1; w <=V_dingdianshu; w++)
                {
                    if (!Final[w] // 未选中
                        && Dist[w] < min) // 值更小
                    {
                        minIndex = w;
                        min = Dist[w];
                    }
                }
                if(minIndex!=-1)
                    s = minIndex;
                Final[s] = true;
                
            }
            return Dist[s];
        }
        // 图的连通性测试
        // 参数start用于指定从哪个点开始找（索引从0开始），这样在一定程序上可以提高程序效率
        // 空图返回真
        // 这里对start功能的定义还应该加上：start点一定要在连通图上
        public Tuple<bool,bool> ConnectivityTest(int start)
        {
            bool bNoPoints = false;
            LinkedList<int> nodeSet=new LinkedList<int>(); // 连通顶点集
            
            LinkedList<int> for_test_nodes=new LinkedList<int>(); // 与新加入连通点连通的未加入点集
            int i, j;
            LinkedList<int> singlePoints=new LinkedList<int>(); // 图中的单点集
            int test = 1;
            int k = 0;

            // 先找出单点
            bool hasEdge = false;
            for (i = 1; i <=V_dingdianshu; i++)
            {
                hasEdge = false;
                for (j = 1; j <=V_dingdianshu; j++) // 这里起始应该是0，不然最后一个点如果是单点则无法判断
                {
                    if (Graph[i,j] > 0)
                    {
                        hasEdge = true;
                        break;
                    }
                }
                if (!hasEdge)
                {
                    singlePoints.AddLast(i);
                }
            }
                
            if(singlePoints.Count == V_dingdianshu)
                bNoPoints = true; // 设置bNoPoints标志

            if (singlePoints.Contains(start)) // start点必须在连通图中
            {
                return Tuple.Create(false,bNoPoints);
            }
            for_test_nodes.AddLast(start); // 

            while (for_test_nodes.Count > 0)
            {
                int testNode = for_test_nodes.Last() ;
                for_test_nodes.RemoveLast();

                for (i = 1; i <=V_dingdianshu; i++)
                {
                    if (Graph[testNode,i] > 0)
                    {
                        if (!nodeSet.Contains(i))
                        {
                            nodeSet.AddLast(i);
                            for_test_nodes.AddLast(i);
                        }
                    }
                }
            }

            for (i = 1; i <=V_dingdianshu; i++)
            {
                if ((!singlePoints.Contains(i)
                    )&& (!nodeSet.Contains(i)))
                // 存在点既不是单点，也不在当前连通顶点集中，则这个点一定在其他连通子图中，返回假
                {
                    return Tuple.Create(false,bNoPoints);
                }
            }

            return Tuple.Create(true, bNoPoints);
        }

        // 测试图中是否有度为奇的顶点，结果保存在中，返回奇度顶点数
        public int OddTest()
        {
            int i, j, rSum, count;

            // 初始化
            for (i = 1; i <=V_dingdianshu; i++)
            {
                Odd_Grouping[i] = 0; // 0表示不为奇
                Bak_Odd_Grouping[i] = 0;
            }
            count = 0;

            for (i = 1; i <=V_dingdianshu; i++)
            {
                rSum = 0;
                for (j = 1; j <=V_dingdianshu; j++)
                {
                    rSum += Graph[i,j]; // 求i行和
                }
                if (rSum % 2 == 1)
                {
                    Odd_Grouping[i] = 1;
                    count++;
                }
            }

            return count;
        }

        public void Bak_Grouping()
        {
            int i;
            for (i = 1; i <=V_dingdianshu; i++)
            {
                Bak_Odd_Grouping[i] = Odd_Grouping[i];
            }
        }

        // 对奇度顶点进行分组，level值从2开始取值。
        // 返回值表示当前这种分组是否是当前所找到中的最好分组。本程序中没有采用其返回值。
        public bool Grouping(int level)
        {
            if (level < 2)
            {
                System.Console.WriteLine("小于2的level值是不允许的。");
                System.Environment.Exit(-1);
            }

            int i, j, findI = -1;
            for (i = 1; i <=V_dingdianshu; i++)
            {
                if (Odd_Grouping[i] == 1)
                {
                    Odd_Grouping[i] = level; // 找到第一个组合点。
                    findI = i;
                    break;
                }
            }

            bool re = true;
            if (findI == -1)  // 这里是形成一对新的组合后的地方，此时应该计算各组合最小路径之和。
            {
                int weightSum = 0;
                for (i = 2; i < level; i++) // 根据level的值可以知道分组的取值是从2到level-1的，所以i如是计数
                {
                    int []index=new int[2];
                    int p = 0;
                    for (j = 1; j <=V_dingdianshu; j++)
                    {
                        if (Odd_Grouping[j] == i)
                        {
                            index[p] = j;
                            if (p == 1) // 设置了第二个index值
                            {
                                break;
                            }
                            p++;
                        }
                    }
                    weightSum += Dijstra(index[0], index[1], true); // 这里暂时只计算最短路权值和，不实际上添加边，最后才添加。这样加边计算只会调用一次。
                }

                if (weightSum < SHORTEST_PATH_WEIGHT) // 当前组合比以往要优，将当前的排列组合情况更新到全局
                {
                    Bak_Grouping(); // 如果当前分组比以往都好，备份一下
                    SHORTEST_PATH_WEIGHT = weightSum;
                    return true; // 找到了更优组合，返回递归调用为真
                }
                else
                {
                    return false; // 没找到了更优组合，返回递归调用为假
                }
            }
            else if (findI > -1)
            {
                // 上面找到了第一个点了，现在从上面继续找第二个点。
                for (/* 继续上面的for */; i <=V_dingdianshu; i++)
                {
                    if (Odd_Grouping[i] == 1) // 找到第二个点
                    {
                        Odd_Grouping[i] = level;
                        re = Grouping(level + 1);
                        Odd_Grouping[i] = 1; // 无论当前分组是不是当前最好分组，我们都还要继续查找剩余分组情况
                    }
                }
            }
            else
            {
                System.Console.WriteLine("findCount值异常");
                System.Environment.Exit(-1);
            }

            if (findI > -1)
            {
                Odd_Grouping[findI] = 1; // 无论当前分组是不是最好分组，我们都还要继续查找剩余分组情况
            }

            return re;
        }

        public void AddShortPath(int from, int to)
        {
            int i, back;

            Dijstra(from, to, false); // 求最短路径，结果在dist数组中
            back = to;
            while (back != from) // from ... back ... to
            {
                for (i = 1; i <=V_dingdianshu; i++)
                {
                    if (i != back
                        && Dist[i] < COST_NO_LINK // from有边到i
                        && Dist[back] < COST_NO_LINK // from有边到back
                        && Dist[i] + Cost[i,back] == Dist[back]) // from通过中继点i再到back的长度恰好等于from到back的长度，即证明点i在最短路径上(注，这里如果(i,back)没有边连接，那么Dist[i] + Cost[i][back]一定为负数)
                    {
                        Graph[i,back]++; // 添加一条边
                        Graph[back,i]++;
                        back = i;
                        break;
                    }
                }
                if (i == V_dingdianshu+1) // 编程常识：这里break后不会再执行++
                {
                    System.Console.WriteLine("程序异常，最短路径出问题了。。。");
                    System.Environment.Exit(-1);
                }
            }
        }

        // 根据odd数组的分组情况添加最短路径
        public void AddShortPaths()
        {
            int i, j;

            for (i = 1; i <=V_dingdianshu; i++)
            {
                if (Bak_Odd_Grouping[i] > 1)
                {
                    for (j = i + 1; j <=V_dingdianshu; j++)
                    {
                        if (Bak_Odd_Grouping[j] == Bak_Odd_Grouping[i])
                        {
                            AddShortPath(i, j);
                            break;
                        }
                    }
                }
            }
        }

        // 处理图中可能存在度为奇的情况
        public void OddDeal()
        {
            // 判断是否存在为奇的点，有的话要处理
            int oddCount = OddTest();
            if (oddCount > 0)
            {
                if (oddCount % 2 == 1)
                {
                    System.Console.WriteLine("这是一个奇怪的图，存在奇数个奇度顶点的连通图吗？");
                    System.Environment.Exit(-1);
                }

                // 对为奇的点进行排列组合。。。
                Grouping(2); // 这里得到的odd2是最优的
                AddShortPaths(); // 根据odd数组添加最短路径
            }
        }
        public int PrintBound(int stano)
        {
            int add = 0;
            foreach (string end in metrosys.StaCollection[NoToName[stano]].EndStas)
            {
                Console.WriteLine(NoToName[stano] + "-" + end);
                Console.WriteLine(end + "-" + NoToName[stano]);
                add += metrosys.SectionLen(metrosys.StaCollection[end], metrosys.StaCollection[NoToName[stano]]) * 2;
            }
            return add;
        }
        /*
        用Fleury算法求最短欧拉回游
        假设迹wi=v0e1v1…eivi已经选定，那么按下述方法从E-｛e1,e2,…,ei｝中选取边ei+1:
        1)、 ei+1与vi+1相关联；
        2)、除非没有别的边可选择，否则 ei+1不能是Gi=G-｛e1,e2,…,ei｝的割边。
        3)、 当(2)不能执行时，算法停止。
        */
        public void Fleury(int start,string string_in,string string_out,int addcount)
        {
            int i;
            int vi = start; // v0e1v1…eivi已经选定
            bool bNoPoints=false, bCnecTest;
            Console.WriteLine("最短路线");
            Console.Write(string_in);
            bool flag = false;
            int sum = 0;
            bool[] boundprinted = new bool[MAX_NODE];
            for (i = 0; i < MAX_NODE; i++)
                boundprinted[i] = false;
            if (metrosys.StaCollection[NoToName[vi]].isBoundary && boundprinted[vi] == false)
            {
                sum += PrintBound(vi);
                boundprinted[vi] = true;
            }
            while (true)
            {
                // 找一条不是割边的边ei+1
                flag = false;
                for (i = 1; i <=V_dingdianshu; i++)
                {
                    if (Graph[vi,i] > 0)
                    {
                        // 假设选定（vi，i）这条边
                        Graph[vi,i]--; // 这里会破坏全局Graph的值，但暂时没影响了，都不用了。
                        Graph[i,vi]--;

                        Tuple<bool, bool> t = ConnectivityTest(i);
                        bCnecTest = t.Item1;
                        bNoPoints = t.Item2;
                        if (!bNoPoints && !bCnecTest) // 这里一定要传i，这是欲选择边的末端，它应该在连通图中
                        {
                            Graph[vi,i]++;
                            Graph[i,vi]++;
                            continue;
                        }
                        // 选定（vi，i）这条边
                        Console.WriteLine(NoToName[vi] + "-" + NoToName[i]);
                        sum += metrosys.SectionLen(metrosys.StaCollection[NoToName[vi]], metrosys.StaCollection[NoToName[i]]);
                        vi = i;
                        if(metrosys.StaCollection[NoToName[vi]].isBoundary&&boundprinted[vi]==false)
                        {
                            sum += PrintBound(vi);
                            boundprinted[vi] = true;
                        }
                        flag = true;
                        break;
                    }
                }
                if (flag==false)
                {
                    for (i = 1; i <= V_dingdianshu; i++)
                    {
                        if (Graph[vi, i] > 0)
                        {
                            Graph[vi, i]--; 
                            Graph[i, vi]--;
                            Console.WriteLine(NoToName[vi] + "-" + NoToName[i]);
                            sum += metrosys.SectionLen(metrosys.StaCollection[NoToName[vi]], metrosys.StaCollection[NoToName[i]]);
                            vi = i;
                            if (metrosys.StaCollection[NoToName[vi]].isBoundary && boundprinted[vi] == false)
                            {
                                sum += PrintBound(vi);
                                boundprinted[vi] = true;
                            }
                            flag = true;
                            break;
                        }
                    }
                    if (flag == false)
                    {
                        Console.Write(string_out);
                        sum += addcount;
                        Console.WriteLine("总长度为:"+sum);
                        break;
                    }
                        
                }
            }
        }
    }
}
