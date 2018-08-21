package net.azurewebsites.expenses_by_klaudia.utils;

import android.util.Base64;

import javax.crypto.SecretKeyFactory;
import javax.crypto.spec.PBEKeySpec;
import java.security.SecureRandom;

public class HashUtil {

    public static String GenerateSalt() {

        SecureRandom random = new SecureRandom();
        byte bytes[] = new byte[16];
        random.nextBytes(bytes);
        return Base64.encodeToString(bytes, Base64.DEFAULT);
    }

    public static String Hash(String password, String salt) {
        try {
            int dkLen = 160;
            int rounds = 10000;
            byte[] saltBytes = Base64.decode(salt, Base64.DEFAULT);
            PBEKeySpec keySpec = new PBEKeySpec(password.toCharArray(), saltBytes, rounds, dkLen);
            SecretKeyFactory factory = SecretKeyFactory.getInstance("PBKDF2WithHmacSHA1");
            byte[] out = factory.generateSecret(keySpec).getEncoded();
            return Base64.encodeToString(out, Base64.DEFAULT);
        }
        catch (Exception ex) {
            return null;
        }

    }
}
