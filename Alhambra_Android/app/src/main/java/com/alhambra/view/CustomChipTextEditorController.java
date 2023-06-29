package com.alhambra.view;

import android.text.Editable;
import android.text.Spanned;
import android.text.TextWatcher;
import android.text.style.ImageSpan;
import android.view.View;
import android.widget.EditText;

import com.alhambra.R;
import com.google.android.material.chip.ChipDrawable;

import java.util.ArrayList;
import java.util.HashSet;


public class CustomChipTextEditorController implements TextWatcher, View.OnFocusChangeListener {

    public EditText editText;
    public char splitChar = ',';
    private int currentNonSpanIndex = 0;
    private ArrayList<String> tokens = new ArrayList<>();

    public CustomChipTextEditorController(EditText editText) {
        this.editText=editText;
        this.editText.setOnFocusChangeListener(this);
        this.editText.addTextChangedListener(this);
    }

    public void clearText(){
        editText.setText("");
        currentNonSpanIndex=0;
        tokens.clear();
    }


    @Override
    public void beforeTextChanged(CharSequence s, int start, int count, int after) {

    }

    @Override
    public void onTextChanged(CharSequence s, int start, int before, int count) {
    }

    //TODO: test
    @Override
    public void afterTextChanged(Editable s) {
        int oldNonSpanIndex = currentNonSpanIndex;
        int tokenIndex = tokens.size()-1;
        int len = s.toString().length(); // do not use s.length(), it includes all inputs, include back space
        editText.removeTextChangedListener(this);
        while(true)
        {
            int i = currentNonSpanIndex;
            while(true) {
                if(i>=len) { // exceed length!
                    break;
                }
                if(s.charAt(i)==splitChar){ // encounter split character ,
                    break;
                }
                i++;
            }
            if(len<=i) { // exceed length
                break;
            }
            if(i==0 || i==currentNonSpanIndex) { // avoid speical case
                break;
            }
            // [span]xxxx,
            //       ^   ^ i
            // nonSpanIndex

            // [span]XXXXxxxx,
            //           ^   ^ i
            // nonSpanIndex
            CharSequence token_cs = s.subSequence(currentNonSpanIndex, i);
            String token = token_cs.toString();
            if(token.length()==0) {
                currentNonSpanIndex = i;
                continue;
            }
            // add chip
            ChipDrawable chip = ChipDrawable.createFromResource(editText.getContext(),R.xml.editable_chip);
            chip.setText(token);
            chip.setBounds(0,0,chip.getIntrinsicWidth(),chip.getIntrinsicHeight());
            ImageSpan span = new ImageSpan(chip);
            s.setSpan(span,currentNonSpanIndex,i, Spanned.SPAN_EXCLUSIVE_EXCLUSIVE);
            tokens.add(token.trim());
            //deleteCharacter(i);
            currentNonSpanIndex = i+1;
        }
        editText.addTextChangedListener(this);
    }

    private void deleteCharacter(int pos) {
        editText.setText(editText.getText().delete(pos-1,pos));
    }

    //no duplication
    public ArrayList<String> getTokens() {
        HashSet<String> realTokens = new HashSet<>();
        String[] splits = editText.getText().toString().split(splitChar + "");
        for(String splitString: splits) {
            String trimedStr = splitString.trim();
            if(trimedStr.length()>0){
                realTokens.add(trimedStr);
            }

        }
        tokens = new ArrayList<>(realTokens);
        return tokens;
    }

    @Override
    public void onFocusChange(View v, boolean hasFocus) {
        if(!hasFocus) {
            this.editText.append(",");
        }
    }
}
