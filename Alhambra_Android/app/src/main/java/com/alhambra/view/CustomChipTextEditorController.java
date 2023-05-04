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
    private int lastLength = 0;
    private int currentNonSpanIndex = 0;
    private ArrayList<String> tokens = new ArrayList<>();

    public CustomChipTextEditorController(EditText editText) {
        this.editText=editText;
        lastLength = editText.getText().length();
        this.editText.setOnFocusChangeListener(this);
        this.editText.addTextChangedListener(this);
    }


    @Override
    public void beforeTextChanged(CharSequence s, int start, int count, int after) {

    }

    @Override
    public void onTextChanged(CharSequence s, int start, int before, int count) {
        int newLength = s.length();
        if(newLength<lastLength-1) {
            //delete the span
            tokens.remove(tokens.get(tokens.size()-1));
            currentNonSpanIndex = newLength;
        }
    }

    //TODO: test
    @Override
    public void afterTextChanged(Editable s) {
        int oldNonSpanIndex = currentNonSpanIndex;
        int tokenIndex = tokens.size()-1;
        int len = s.length();
        editText.removeTextChangedListener(this);
        while(true)
        {
            int i = currentNonSpanIndex;
            while(i<len && s.charAt(i)!=splitChar) {
                i++;
            }
            if(len<=i) {
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
            // add chip
            ChipDrawable chip = ChipDrawable.createFromResource(editText.getContext(), R.xml.editable_chip);
            chip.setText(token);
            chip.setBounds(0,0,chip.getIntrinsicWidth(),chip.getIntrinsicHeight());
            ImageSpan span = new ImageSpan(chip);
            s.setSpan(span,currentNonSpanIndex,i-1, Spanned.SPAN_EXCLUSIVE_EXCLUSIVE);
            tokens.add(token.trim());
            deleteCharacter(i);
            currentNonSpanIndex = i;
        }
        editText.addTextChangedListener(this);
    }

    private void deleteCharacter(int pos) {
        editText.setText(editText.getText().delete(pos-1,pos));
    }

    //no duplication
    public HashSet<String> getTokens() {
        return new HashSet<>(tokens);
    }

    @Override
    public void onFocusChange(View v, boolean hasFocus) {
        if(!hasFocus) {
            this.editText.append(",");
        }
    }
}
