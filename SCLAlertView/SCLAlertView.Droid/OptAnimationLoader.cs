using System;
using Android.Content;
using Android.Content.Res;
using Android.Util;
using Android.Views.Animations;
using Java.IO;
using Org.XmlPull.V1;
using System.Xml;
using Java.Lang;

namespace SCLAlertView.Droid
{
	public class OptAnimationLoader
	{
		public static Animation loadAnimation(Context context, int id) {

			XmlReader parser = null;
			try {
				parser = context.Resources.GetAnimation(id);
				return createAnimationFromXml(context, parser);
			} catch (XmlPullParserException ex) {
				Resources.NotFoundException rnf = new Resources.NotFoundException("Can't load animation resource ID #0x" +Integer.ToHexString(id));
				rnf.InitCause(ex);
				throw rnf;
			} catch (IOException ex) {
				Resources.NotFoundException rnf = new Resources.NotFoundException("Can't load animation resource ID #0x" +Integer.ToHexString(id));
				rnf.InitCause(ex);
				throw rnf;
			} finally {
				if (parser != null) parser.Close();
			}
		}

		private static Animation createAnimationFromXml(Context c, XmlReader parser) {

			return createAnimationFromXml(c, parser, null, Xml.AsAttributeSet(parser));
		}

		private static Animation createAnimationFromXml(Context c, XmlReader parser, AnimationSet parent, IAttributeSet attrs){

			Animation anim = null;

			// Make sure we are on a start tag.
			int type;
			int depth = parser.Depth;

			while (((type=parser.read()) != XmlReader. || parser.getDepth() > depth)
			   && type != XmlPullParser.END_DOCUMENT) {

				if (type != XmlPullParser.START_TAG) {
					continue;
				}

				String  name = parser.getName();

				if (name.equals("set")) {
					anim = new AnimationSet(c, attrs);
					createAnimationFromXml(c, parser, (AnimationSet)anim, attrs);
				} else if (name.equals("alpha")) {
					anim = new AlphaAnimation(c, attrs);
				} else if (name.equals("scale")) {
					anim = new ScaleAnimation(c, attrs);
				}  else if (name.equals("rotate")) {
					anim = new RotateAnimation(c, attrs);
				}  else if (name.equals("translate")) {
					anim = new TranslateAnimation(c, attrs);
				} else {
					try {
						anim = (Animation) Class.forName(name).getConstructor(Context.class, AttributeSet.class).newInstance(c, attrs);
					} catch (Exception te) {
						throw new RuntimeException("Unknown animation name: " + parser.getName() + " error:" + te.getMessage());
					}
				}

				if (parent != null) {
					parent.addAnimation(anim);
				}
			}

			return anim;

		}
	}
}
