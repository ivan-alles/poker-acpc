/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package ai.pkr.jmetabots;

import java.io.*;

/**
 *
 * @author alles
 */
public class Header {
        public int FunctionId;
        public int DataLength;

        public static int SIZE = 8;

        public void writeTo(OutputStream s) throws IOException
        {
            s.write((FunctionId >> 24) & 0xFF);
            s.write((FunctionId >> 16) & 0xFF);
            s.write((FunctionId >> 8) & 0xFF);
            s.write((FunctionId) & 0xFF);

            s.write((DataLength >> 24) & 0xFF);
            s.write((DataLength >> 16) & 0xFF);
            s.write((DataLength >> 8) & 0xFF);
            s.write((DataLength) & 0xFF);
        }

        public static Header readFrom(InputStream s) throws IOException
        {
            Header h = new Header();
            
            h.FunctionId = (s.read() << 24) |
                            (s.read() << 16) |
                            (s.read() << 8) |
                            (s.read());

            h.DataLength = (s.read() << 24) |
                            (s.read() << 16) |
                            (s.read() << 8) |
                            (s.read());
            return h;
        }

}
