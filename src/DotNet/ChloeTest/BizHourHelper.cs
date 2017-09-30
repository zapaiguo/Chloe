using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeTest
{
    public static class BizHourHelper
    {
        public static long FullDay { get; private set; }

        static BizHourHelper()
        {
            FullDay = Get24小时营业();
        }

        public static long ProccessValue(string value)
        {
            if (value == null)
                throw new ArgumentNullException();

            if (value.Trim() == "0")
                return 0;

            if (value == string.Empty)
                throw new ArgumentException("营业时间不能为空，如店铺不开则填 0");

            long powFlag = 0;

            string[] arr = value.Split(';');
            foreach (var item in arr)
            {
                string[] arr1 = item.Split('-');
                int startFlag = ConvertMoment(arr1[0]);
                int endFlag = ConvertMoment(arr1[1]);
                if (startFlag >= endFlag)
                    throw new ArgumentException(string.Format("结束时间必须大于起始时间", item));

                for (int i = startFlag; i < endFlag; i++)
                {
                    powFlag = powFlag | Convert.ToInt64((Math.Pow(2, i)));
                }
            }

            return powFlag;
        }

        /// <summary>
        /// 00:00 
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static List<TimeInterval> ConvertToTimeInterval(long flag)
        {
            /*
              * 00:00-00:30 -> 0 -> 2^0
              * 00:30-01:00 -> 1 -> 2^1
              * 01:00-01:30 -> 2 -> 2^2
              * 01:30-02:00 -> 3 -> 2^3
              * 02:00-02:30 -> 4 -> 2^4
                    .
                    .
                    .
              * 23:30-24:00 -> 47 -> 2^47
              */


            List<TimeInterval> arr = new List<TimeInterval>();

            if (flag == 0)
                return arr;

            int start = -1;
            int end = -1;

            for (int i = 0; i < 48; i++)
            {
                long f = Convert.ToInt64(Math.Pow(2, i));
                long a = flag & f;

                if (a == f)
                {
                    if (start == -1)
                    {
                        start = i;
                        end = i + 1;
                    }
                    else
                    {
                        end++;
                    }
                }
                else
                {
                    if (start != -1)
                    {
                        arr.Add(new TimeInterval(start, end));
                    }

                    start = -1;
                    end = -1;
                }
            }

            if (start != -1)
            {
                arr.Add(new TimeInterval(start, end));
            }

            return arr;
        }
        public static string ConvertToTimeIntervalString(long flag)
        {
            var list = ConvertToTimeInterval(flag);
            string s = "";
            string c = "";
            foreach (var item in list)
            {
                s += c + item.ToString();
                c = ";";
            }

            return s;
        }
        static int ConvertMoment(string s)
        {
            string[] timeStrings = s.Split(':');
            if (timeStrings.Length != 2)
                throw new ArgumentException(string.Format("解析出错：{0},请输入诸如 10:30 的格式", s));

            string hString = timeStrings[0];
            string mString = timeStrings[1];

            int h, m;
            if (!int.TryParse(hString, out h))
                throw new ArgumentException(string.Format("解析出错：{0},请输入诸如 10:30 的格式", s));

            if (!int.TryParse(mString, out m))
                throw new ArgumentException(string.Format("解析出错：{0},请输入诸如 10:30 的格式", s));

            if (h < 0 || h > 24)
                throw new ArgumentException(string.Format("请输入正确的小时 {0}", s));
            if (m != 0 && m != 30)
                throw new ArgumentException(string.Format("分钟段请输入 0 或者 30", s));

            int flag = h * 2;
            if (m == 30)
                flag++;

            return flag;
        }

        static long Get24小时营业()
        {
            long flag = 0;
            for (int i = 0; i < 48; i++)
            {
                flag = flag | Convert.ToInt64(Math.Pow(2, i));
            }

            return flag;
        }

        public static string BizHourToDialect(long t1, long t2)
        {
            List<string> timeStrings = new List<string>();

            List<TimeInterval> timeInterval1 = BizHourHelper.ConvertToTimeInterval(t1);
            List<TimeInterval> timeInterval2 = BizHourHelper.ConvertToTimeInterval(t2);
            long _24dian = Convert.ToInt64(Math.Pow(2, 47));
            long _0dian = Convert.ToInt64(Math.Pow(2, 0));
            if (t1 == BizHourHelper.FullDay && t2 == BizHourHelper.FullDay)
            {
                return "24小时";
                ////就取 t1 
                //foreach (var item in timeInterval1)
                //{
                //    timeStrings.Add(item.ToString());
                //}
            }
            else if (t1 == t2)
            {
                if ((t1 & _24dian) == _24dian && (t1 & _0dian) == _0dian)
                {
                    //说明是跨天，如 0:00-2:00;17:30-24:00 此时转为 17:30-次日2:00
                    //使用次日
                    TimeInterval ti = timeInterval1.Last();
                    string s = TimeInterval.ConvertTimeString(ti.Start);
                    string ciriEnd = TimeInterval.ConvertTimeString(timeInterval1.First().End);
                    string ss = s + "-次日" + ciriEnd;

                    for (int i = 1; i < timeInterval1.Count - 1; i++)
                    {
                        TimeInterval ti1 = timeInterval1[i];
                        timeStrings.Add(ti1.ToString());
                    }

                    timeStrings.Add(ss);
                }
                else
                {
                    //就取 t1
                    foreach (var item in timeInterval1)
                    {
                        timeStrings.Add(item.ToString());
                    }
                }
            }
            else if ((t1 & _24dian) == _24dian && (t2 & _0dian) == _0dian) //说明跨天
            {
                if ((t1 & _0dian) != _0dian)
                {
                    //使用次日
                    TimeInterval ti = timeInterval1.Last();
                    string s = TimeInterval.ConvertTimeString(ti.Start);
                    string ciriEnd = TimeInterval.ConvertTimeString(timeInterval2.First().End);
                    string ss = s + "-次日" + ciriEnd;

                    for (int i = 0; i < timeInterval1.Count - 1; i++)
                    {
                        TimeInterval ti1 = timeInterval1[i];
                        timeStrings.Add(ti1.ToString());
                    }
                    timeStrings.Add(ss);
                }
                else
                {
                    //就取 t1
                    foreach (var item in timeInterval1)
                    {
                        timeStrings.Add(item.ToString());
                    }
                }
            }
            else
            {
                //不夸天，就取 t1
                foreach (var item in timeInterval1)
                {
                    timeStrings.Add(item.ToString());
                }
            }

            var c = "";
            var ret = "";
            for (var i = 0; i < timeStrings.Count; i++)
            {
                ret += c;
                ret += timeStrings[i];
                c = ";";
            }

            return ret;
        }

        /// <summary>
        /// 解析 bizHoursText ，转成 List<TimeMomentRange> 形式。如果 bizHoursText 为空，则返回空集合
        /// </summary>
        /// <param name="bizHoursText"></param>
        /// <returns></returns>
        public static List<TimeMomentRange> ConvertToTimeMomentRanges(string bizHoursText)
        {
            List<TimeMomentRange> retList = new List<TimeMomentRange>();

            if (string.IsNullOrEmpty(bizHoursText))
                return retList;

            /* 8:00-12:00;14:00-20:00，18:00-2:00（表示跨天） */
            string[] arr = bizHoursText.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string item in arr)
            {
                //item: 8:00-20:00
                TimeMomentRange rmRange = TimeMomentRange.Create(item);
                retList.Add(rmRange);
            }

            return retList;
        }

        public static bool IsLegalBizHoursText(string bizHoursText)
        {
            /* 检查输入的格式是否合法 */
            /* 8:00-12:00;14:00-20:00，18:00-2:00（表示跨天） */

            List<TimeMomentRange> list;
            return TryParseBizHoursText(bizHoursText, out list);
        }
        public static bool TryParseBizHoursText(string bizHoursText, out  List<TimeMomentRange> list)
        {
            /* 检查输入的格式是否合法 */
            /* 8:00-12:00;14:00-20:00，18:00-2:00（表示跨天） */

            list = null;
            try
            {
                list = ConvertToTimeMomentRanges(bizHoursText);
                return true;
            }
            catch (ArgumentException ex)
            {
                return false;
            }


            throw new NotImplementedException();
        }

        public static List<TimeMomentRange> ConvertToTimeMomentRanges(long flag)
        {
            List<TimeInterval> tis = ConvertToTimeInterval(flag);

            List<TimeMomentRange> tmrs = new List<TimeMomentRange>();

            foreach (TimeInterval item in tis)
            {
                TimeMoment startTimeMoment = TimeInterval.ConvertTimeMoment(item.Start);
                TimeMoment endTimeMoment = TimeInterval.ConvertTimeMoment(item.End);

                TimeMomentRange tmr = new TimeMomentRange(startTimeMoment, endTimeMoment);

                tmrs.Add(tmr);
            }

            TimeMomentRange tmg1 = tmrs.Where(a => a.End.GetTotalMinute() == (24 * 60)).FirstOrDefault();
            TimeMomentRange tmg2 = tmrs.Where(a => a.Start.GetTotalMinute() == 0).FirstOrDefault();

            if (tmg1 != null && tmg2 != null)
            {
                TimeMomentRange newTmg = new TimeMomentRange(tmg1.Start, tmg2.End);
                tmrs.Remove(tmg1);
                tmrs.Remove(tmg2);
                tmrs.Add(newTmg);
            }

            return tmrs;

            throw new NotImplementedException();
        }
        /// <summary>
        /// 拼接成 8:00-24:00    8:00-12:00;14:00-20:30   8:00-12:00;18:00-2:00   等字符串格式
        /// </summary>
        /// <param name="tmrs"></param>
        public static string ToBizHoursText(List<TimeMomentRange> tmrs)
        {
            string bizHoursText = null;

            if (tmrs == null || tmrs.Count == 0)
                return bizHoursText;

            string c = "";
            foreach (TimeMomentRange item in tmrs)
            {
                bizHoursText += c + item.ToString();
                c = ";";
            }

            return bizHoursText;
        }

        /// <summary>
        /// 拼接成 8:00-24:00    8:00-12:00;14:00-20:30   8:00-12:00;18:00-2:00   等字符串格式
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static string ToBizHoursText(long flag)
        {
            List<TimeMomentRange> tmrs = BizHourHelper.ConvertToTimeMomentRanges(flag);
            string bizHoursText = BizHourHelper.ToBizHoursText(tmrs);

            return bizHoursText;
        }
    }

    /// <summary>
    /// 表示一个时间段：HH:mm-HH:mm  10:00-14:00。如果 End 小于 Start 则表示跨天，如：18:00-2:00
    /// </summary>
    public class TimeMomentRange
    {
        /// <summary>
        /// 如果 end 是 00:00，则将 00:00 转成 24:00
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public TimeMomentRange(TimeMoment start, TimeMoment end)
        {
            this.Start = start;

            if (end.Hour == 0 && end.Minute == 0)
                this.End = new TimeMoment(24, 0);/* 如果 end 是 00:00，则将 00:00 转成 24:00 */
            else
                this.End = end;
        }

        /// <summary>
        /// 如果 End 小于 Start 则表示跨天
        /// </summary>
        public TimeMoment Start { get; private set; }
        public TimeMoment End { get; private set; }

        /// <summary>
        /// 相对 0 点的分钟差
        /// </summary>
        /// <returns></returns>
        public int GetRelativeStartMinutes()
        {
            return this.Start.GetTotalMinute();
        }
        /// <summary>
        /// 相对 0 点的分钟差
        /// </summary>
        /// <returns></returns>
        public int GetRelativeEndMinutes()
        {
            var start = this.Start.GetTotalMinute();
            var end = this.End.GetTotalMinute();

            if (end < start)
            {
                /* 表示跨天，这加上 24 小时 */
                return end + (24 * 60);/* 加一天的意思 */
            }

            return end;
        }

        public TimeRange ToDateTimeRange(DateTime date)
        {
            DateTime d = date.Date;
            DateTime start = d.Add(TimeSpan.FromMinutes(this.Start.GetTotalMinute()));
            DateTime end;

            if (this.Start.GetTotalMinute() > this.End.GetTotalMinute())
            {
                /* 跨天 */
                end = d.AddHours(24).AddMinutes(this.End.GetTotalMinute());
            }
            else
                end = d.AddMinutes(this.End.GetTotalMinute());

            var ret = new TimeRange(start, end);
            return ret;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", this.Start.ToString(), this.End.ToString());
        }

        /// <summary>
        /// 将 tmrs 按 Start 时间升序。如果出现时间有重合，则合并一起。如：11:00-21:00;11:00-22:00 这种情况会合并成 11:00-22:00。
        /// </summary>
        /// <param name="trs"></param>
        /// <returns></returns>
        public static List<TimeMomentRange> OrderAndMerge(List<TimeMomentRange> tmrs)
        {
            if (tmrs == null)
                return null;

            List<TimeMomentRange> retList = new List<TimeMomentRange>(tmrs.Count);

            var en = tmrs.OrderBy(a => a.GetRelativeStartMinutes());

            foreach (var tmr in en)
            {
                TimeMomentRange lastTmr = retList.LastOrDefault();
                if (lastTmr == null)
                {
                    retList.Add(tmr);
                    continue;
                }

                if (tmr.GetRelativeStartMinutes() <= lastTmr.GetRelativeEndMinutes())
                {
                    if (tmr.GetRelativeEndMinutes() > lastTmr.GetRelativeEndMinutes())
                    {
                        lastTmr.End = tmr.End;
                    }
                }
                else
                {
                    retList.Add(tmr);
                }
            }

            return retList;
        }

        public static TimeMomentRange Create(string text)//text: 8:00-20:00
        {
            string[] timeRange = text.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

            if (timeRange.Length != 2)
                throw new ArgumentException();

            TimeMoment tm1 = TimeMoment.Create(timeRange[0]);
            TimeMoment tm2 = TimeMoment.Create(timeRange[1]);

            TimeMomentRange rmRange = new TimeMomentRange(tm1, tm2);
            return rmRange;
        }
        public static List<TimeMomentRange> CreateMultiple(string text)//text: 8:00-12:00;14:00-20:00
        {
            List<TimeMomentRange> retList = new List<TimeMomentRange>();

            if (string.IsNullOrEmpty(text))
                return retList;

            string[] arr = text.Split(';');

            foreach (string item in arr)
            {
                retList.Add(TimeMomentRange.Create(item));
            }

            return retList;
        }
        public static string ConvertToBizHoursText(List<TimeMomentRange> tmrs)
        {
            string ret = null;

            if (tmrs == null || tmrs.Count == 0)
                return ret;

            string c = "";
            foreach (TimeMomentRange item in tmrs)
            {
                ret += c + item.ToString();
                c = ";";
            }

            return ret;
        }
    }

    /// <summary>
    /// 08:00,12:30
    /// </summary>
    public struct TimeMoment
    {
        int _hour;
        int _minute;
        public TimeMoment(int hour, int minute)
        {
            if ((hour < 0 || hour > 24) || (minute < 0 || minute > 60))
                throw new ArgumentException();

            this._hour = hour;
            this._minute = minute;
        }
        public int Hour { get { return this._hour; } }
        public int Minute { get { return this._minute; } }

        public int GetTotalMinute()
        {
            return this.Hour * 60 + this.Minute;
        }
        public bool Equals(TimeMoment obj)
        {
            return this.GetTotalMinute() == obj.GetTotalMinute();
        }
        public override string ToString()
        {
            //08:00
            return string.Format("{0}:{1}", this.Hour.ToString("D2"), this.Minute.ToString("D2"));
        }
        public static TimeMoment Create(string text/* 8:00 */)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException();

            string[] hm = text.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (hm.Length != 2)
                throw new ArgumentException();

            int? h = hm[0].ToInt32();
            int? m = hm[1].ToInt32();
            if (h == null || m == null || (h.Value < 0 || h.Value > 24) || (m.Value < 0 || m.Value > 60))
                throw new ArgumentException();

            return new TimeMoment(h.Value, m.Value);
        }
    }

    public struct TimeInterval
    {
        /*
             * 00:00-00:30 -> 0 -> 2^0
             * 00:30-01:00 -> 1 -> 2^1
             * 01:00-01:30 -> 2 -> 2^2
             * 01:30-02:00 -> 3 -> 2^3
             * 02:00-02:30 -> 4 -> 2^4
                   .
                   .
                   .
             * 23:30-24:00 -> 47 -> 2^47
         */


        int _start;
        int _end;
        public TimeInterval(int start, int end)
        {
            if (end < start)
                throw new ArgumentException();

            this._start = start;
            this._end = end;
        }

        /// <summary>
        /// 0 到 47 之间的一个数
        /// </summary>
        public int Start { get { return this._start; } }
        /// <summary>
        /// 0 到 47 之间的一个数
        /// </summary>
        public int End { get { return this._end; } }

        public override string ToString()
        {
            string start = ConvertTimeString(this.Start);
            string end = ConvertTimeString(this.End);

            return start + "-" + end;
        }

        public static string ConvertTimeString(int flag)
        {
            string hour, munite;
            if (flag % 2 == 0)
            {
                //说明是 0 2 4....
                hour = (flag / 2).ToString();
                munite = "00";
            }
            else
            {
                hour = ((flag - 1) / 2).ToString();
                munite = "30";
            }

            return hour + ":" + munite;
        }

        public static TimeMoment ConvertTimeMoment(int flag)
        {
            int hour, munite;
            if (flag % 2 == 0)
            {
                //说明是 0 2 4....
                hour = (flag / 2);
                munite = 0;
            }
            else
            {
                hour = ((flag - 1) / 2);
                munite = 30;
            }

            return new TimeMoment(hour, munite);
        }

        public TimeRange ToTimeRange(DateTime date)
        {
            DateTime d = date.Date;
            var start = d.Add(TimeSpan.FromMinutes(this.Start * 30));
            var end = d.Add(TimeSpan.FromMinutes(this.End * 30));

            var ret = new TimeRange(start, end);
            return ret;
        }
    }

    public class TimeRange
    {
        public TimeRange(DateTime start, DateTime end)
        {
            this.Start = start;
            this.End = end;
        }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public override string ToString()
        {
            return string.Format("{0}-{1}", this.Start, this.End);
        }

        /// <summary>
        /// 将 trs 按 Start 时间升序。如果出现时间有重合，则合并一起
        /// </summary>
        /// <param name="trs"></param>
        /// <returns></returns>
        public static List<TimeRange> OrderAndMerge(List<TimeRange> trs)
        {
            List<TimeRange> retList = new List<TimeRange>(trs.Count);

            foreach (var tr in trs.OrderBy(a => a.Start))
            {
                TimeRange lastTr = retList.LastOrDefault();
                if (lastTr == null)
                {
                    retList.Add(tr);
                    continue;
                }

                /* 判断 lastTr 的时间是否有重合，如果有，则合并 */

                if (tr.Start <= lastTr.End)
                {
                    if (tr.End > lastTr.End)
                    {
                        lastTr.End = tr.End;
                    }
                }
                else
                {
                    retList.Add(tr);
                }
            }

            return retList;
        }
    }
}
