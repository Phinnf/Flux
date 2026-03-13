'use client';

import Quill from 'quill';
import { useEffect, useRef, useState } from 'react';

interface RendererProps {
  value: string;
}

const Renderer = ({ value }: RendererProps) => {
  const [isEmpty, setIsEmpty] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!containerRef.current) return;

    const container = containerRef.current;

    const quill = new Quill(document.createElement('div'), {
      theme: 'snow',
    });

    quill.enable(false);

    try {
      const contents = JSON.parse(value);
      quill.setContents(contents);

      const isEmpty = quill.getText().replace(/<(.|\n)*?>/g, '').trim().length === 0;
      setIsEmpty(isEmpty);

      container.innerHTML = quill.root.innerHTML;
    } catch {
      // If it's not JSON (plain text), just render it
      container.innerText = value;
    }

    return () => {
      if (container) container.innerHTML = '';
    };
  }, [value]);

  if (isEmpty) return null;

  return <div ref={containerRef} className="ql-editor ql-renderer" />;
};

export default Renderer;
